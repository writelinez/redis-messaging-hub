using RedisMessagingHub.Entities;
using RedisMessagingHub.Services;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace RedisMessagingHub
{
    public interface IRedisMessageHub
    {
        WebSocket Socket { get; set; }

        bool Authenticate { get; }

        Task OnConnectionEstablished(RedisUserInstance redisClient);

        Task<bool> OnAuthenticate(Message authenticationToken);

        Task OnPing(Message message);

        Task OnPong(Message message);

        Task OnIncomingMessage(Message message);

        Task OnChannelSubscribed(Message message);

        Task OnChannelUnsubscribed(Message message);

        Task OnConnectionClosed(RedisUserInstance connection);
    }
}
