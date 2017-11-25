using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RedisMessagingHub.Entities;
using RedisMessagingHub.Enums;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisMessagingHub.Services
{
    public class WebSocketService
    {
        private const short MAX_BUFFER_LEN = 16384;
        private const int SOCKET_TIMEOUT = 60000;
        private const int PING_INTERVAL = 30000;

        private readonly IRedisMessageHub _hub = null;
        private readonly RedisService _redisService = null;

        private ArraySegment<byte> SegmentInputBuffer;
        private ArraySegment<byte> SegmentOutputBuffer;
        private RedisUserInstance redisUserInstance = null;

        private DateTime lastPong = DateTime.MinValue;

        public WebSocketService(IRedisMessageHub hub, RedisService redisService)
        {
            _redisService = redisService;
            _hub = hub;
        }

        public async Task StartSocketListener(HttpContext context)
        {
            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
            _hub.Socket = socket;

            try
            {
                lastPong = DateTime.Now;

                redisUserInstance = new RedisUserInstance(_redisService);

                await _hub.OnConnectionEstablished(redisUserInstance);

                if ((!_hub.Authenticate) || (_hub.Authenticate && (await _hub.OnAuthenticate(await WaitForAuthenticationMessage(socket)))))
                {                   
                    Task.WaitAny
                            (
                                MessageInputHandler(socket),
                                ValidateConnection(socket)
                            );
                }
            }
            finally
            {
                await _hub.OnConnectionClosed(redisUserInstance);
                if (redisUserInstance != null)
                {
                    redisUserInstance.Dispose();
                    redisUserInstance = null;
                }
            }
        }

        private async Task MessageInputHandler(WebSocket socket)
        {
            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    CancellationTokenSource ct = new CancellationTokenSource();
                    ct.CancelAfter(SOCKET_TIMEOUT);

                    SegmentInputBuffer = new ArraySegment<byte>(new byte[MAX_BUFFER_LEN]);

                    WebSocketReceiveResult result = await socket.ReceiveAsync(SegmentInputBuffer, ct.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        byte[] buffer = new byte[result.Count];
                        Array.Copy(SegmentInputBuffer.Array, buffer, buffer.Length);

                        Message message = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(buffer));
                        //Send message to the correct handler
                        switch (message.Type)
                        {
                            case MessageType.PING_PONG:
                                await _hub.OnPong(message);
                                HandlePong(socket);
                                break;
                            case MessageType.MESSAGE:
                                await _hub.OnIncomingMessage(message);
                                await HandleIncomingMessage(socket, message);
                                break;
                            case MessageType.SUBSCRIBE:
                                await _hub.OnChannelSubscribed(message);
                                await SubscribeToChannel(socket, message);
                                break;
                            case MessageType.UNSUBSCRIBE:
                                await _hub.OnChannelUnsubscribed(message);
                                await UnsubscribeFromChannel(socket, message);
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    socket.CloseOutputAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None).GetAwaiter().GetResult();
                    socket.Abort();
                    throw ex;
                }
            }
        }

        private void HandlePong(WebSocket socket)
        {
            lastPong = DateTime.Now;
        }

        private async Task HandleIncomingMessage(WebSocket socket, Message message)
        {
            if (!string.IsNullOrEmpty(message.Channel))
            {
                string msg = JsonConvert.SerializeObject(message);
                await redisUserInstance.PublishToChannel(message.Channel, msg);
            }
        }

        private async Task SubscribeToChannel(WebSocket socket, Message message)
        {
            if (!string.IsNullOrEmpty(message.Channel))
            {
                await redisUserInstance.SubscribeToChannel(message.Channel, (channel, msg) => 
                {
                    SegmentOutputBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                    socket.SendAsync(SegmentOutputBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                });

                message.Type = MessageType.SUBSCRIBE_CONFIRM;
                string smsg = JsonConvert.SerializeObject(message);
                SegmentOutputBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(smsg));
                await socket.SendAsync(SegmentOutputBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async Task UnsubscribeFromChannel(WebSocket socket, Message message)
        {
            if (!string.IsNullOrEmpty(message.Channel))
            {
                await redisUserInstance.UnsubscribeFromChannel(message.Channel);

                message.Type = MessageType.UNSUBSCRIBE_CONFIRM;
                string smsg = JsonConvert.SerializeObject(message);
                SegmentOutputBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(smsg));
                await socket.SendAsync(SegmentOutputBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async Task ValidateConnection(WebSocket socket)
        {
            while (socket.State == WebSocketState.Open)
            {
                CancellationTokenSource ct = new CancellationTokenSource();
                ct.CancelAfter(SOCKET_TIMEOUT);

                Message message = new Message();
                message.Id = 0;
                message.Channel = "";
                message.Data = "PING";
                message.Type = MessageType.PING_PONG;

                await _hub.OnPing(message);

                ArraySegment<byte> segBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));

                await socket.SendAsync(segBuffer, WebSocketMessageType.Text, true, ct.Token);

                if (DateTime.Now.Subtract(lastPong).Milliseconds > SOCKET_TIMEOUT)
                {
                    await socket.CloseOutputAsync(WebSocketCloseStatus.Empty, "Client Unreachable", CancellationToken.None);
                    socket.Abort();
                }

                await Task.Delay(PING_INTERVAL);
            }
        }

        private async Task<Message> WaitForAuthenticationMessage(WebSocket socket)
        {
            Message message = null;

            CancellationTokenSource ct = new CancellationTokenSource(SOCKET_TIMEOUT);
            SegmentInputBuffer = new ArraySegment<byte>(new byte[MAX_BUFFER_LEN]);

            try
            {
                var result = await socket.ReceiveAsync(SegmentInputBuffer, ct.Token);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    byte[] buffer = new byte[result.Count];
                    Array.Copy(SegmentInputBuffer.Array, buffer, buffer.Length);

                    message = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(buffer));
                    if (message.Type != MessageType.AUTHENTICATE)
                    {
                        throw new Exception("Message received was not an authentication message.");
                    }
                }
            }
            catch (Exception ex)
            {
                socket.CloseOutputAsync(WebSocketCloseStatus.InternalServerError, $"Authentication Step Error : [{ex.Message}]", CancellationToken.None).GetAwaiter().GetResult();
                socket.Abort();

                throw ex;
            }

            return message;
        }
    }
}
