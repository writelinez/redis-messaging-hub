using RedisMessagingHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedisMessagingHub.Entities;
using RedisMessagingHub.Services;
using RedisMessagingHub.Contracts;
using RedisMessagingHub.Enums;
using Newtonsoft.Json;
using System.Text;
using System.Threading;

namespace WebTest
{
    public class MessageHub : RedisMessageHubBase
    {
        public override bool Authenticate => true;

        public override async Task OnConnectionEstablished(RedisUserInstance redisClient)
        {
            await base.OnConnectionEstablished(redisClient);
        }

        public override async Task<bool> OnAuthenticate(Message authenticationToken)
        {
            Message resp = new Message();
            resp.Data = "Authentication Complete.";
            resp.Date = DateTime.Now;
            resp.Type = MessageType.MESSAGE;

            byte[] dta = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(resp));

            await Socket.SendAsync(new ArraySegment<byte>(dta), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);

            return await base.OnAuthenticate(authenticationToken);
        }

        public override async Task OnIncomingMessage(Message message)
        {
            await base.OnIncomingMessage(message);
        }

        public override async Task OnChannelSubscribed(Message message)
        {
            await base.OnChannelSubscribed(message);
        }

        public override async Task OnChannelUnsubscribed(Message message)
        {
            await base.OnChannelUnsubscribed(message);
        }

        public override async Task OnPing(Message message)
        {
            await base.OnPing(message);
        }

        public override async Task OnPong(Message message)
        {
            await base.OnPong(message);
        }

        public override async Task OnConnectionClosed(RedisUserInstance connection)
        {
            await base.OnConnectionClosed(connection);
        }
    }
}
