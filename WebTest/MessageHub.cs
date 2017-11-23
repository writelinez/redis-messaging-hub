using RedisMessagingHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedisMessagingHub.Entities;
using RedisMessagingHub.Services;
using RedisMessagingHub.Contracts;

namespace WebTest
{
    public class MessageHub : RedisMessageHubBase
    {
        public override bool Authenticate => true;

        public override Task OnConnectionEstablished(RedisUserInstance redisClient)
        {
            return base.OnConnectionEstablished(redisClient);
        }

        public override async Task<bool> OnAuthenticate(Message authenticationToken)
        {
            return await base.OnAuthenticate(authenticationToken);
        }

        public override Task OnIncomingMessage(Message message)
        {
            return base.OnIncomingMessage(message);
        }

        public override Task OnChannelSubscribed(Message message)
        {
            return base.OnChannelSubscribed(message);
        }

        public override Task OnChannelUnsubscribed(Message message)
        {
            return base.OnChannelUnsubscribed(message);
        }

        public override Task OnPing(Message message)
        {
            return base.OnPing(message);
        }

        public override Task OnPong(Message message)
        {
            return base.OnPong(message);
        }

        public override Task OnConnectionClosed(RedisUserInstance connection)
        {
            return base.OnConnectionClosed(connection);
        }
    }
}
