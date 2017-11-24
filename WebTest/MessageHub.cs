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

        public override async Task OnConnectionEstablished(RedisUserInstance redisClient)
        {
            await base.OnConnectionEstablished(redisClient);
        }

        public override async Task<bool> OnAuthenticate(Message authenticationToken)
        {
            //Make sure the message type is for authentication, otherwise we kick the user out.
            if (authenticationToken.Type == RedisMessagingHub.Enums.MessageType.AUTHENTICATE)
            {
                //We base64 encode the auth credentials on the client
                //so we will need to decode them here in our example.
                string rawData = Convert.ToString(authenticationToken.Data);
                byte[] byteData = Convert.FromBase64String(rawData);
                string decodedData = System.Text.Encoding.UTF8.GetString(byteData);

                //If the password matches, then we can let them through, otherwise we kick them.
                if (!decodedData.Equals("MyVeryCoolPassword"))
                    return false;
            }
            else
            {
                return false;
            }
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
