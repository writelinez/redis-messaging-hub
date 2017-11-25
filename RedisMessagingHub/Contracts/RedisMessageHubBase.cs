using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using RedisMessagingHub.Entities;
using RedisMessagingHub.Services;

namespace RedisMessagingHub.Contracts
{
    public abstract class RedisMessageHubBase : IRedisMessageHub
    {
        public virtual bool Authenticate => false;

        public WebSocket Socket { get; set; }

        public virtual async Task<bool> OnAuthenticate(Message authenticationToken)
        {
            await Task.Delay(0);
            return true;
        }

        public virtual async Task OnChannelSubscribed(Message message)
        {
            await Task.Delay(0);
        }

        public virtual async Task OnChannelUnsubscribed(Message message)
        {
            await Task.Delay(0);
        }

        public virtual async Task OnConnectionClosed(RedisUserInstance connection)
        {
            await Task.Delay(0);
        }

        public virtual async Task OnConnectionEstablished(RedisUserInstance redisClient)
        {
            await Task.Delay(0);
        }

        public virtual async Task OnIncomingMessage(Message message)
        {
            await Task.Delay(0);
        }

        public virtual async Task OnPing(Message message)
        {
            await Task.Delay(0);
        }

        public virtual async Task OnPong(Message message)
        {
            await Task.Delay(0);
        }
    }
}
