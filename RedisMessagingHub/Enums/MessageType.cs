using System;
using System.Collections.Generic;
using System.Text;

namespace RedisMessagingHub.Enums
{
    public enum MessageType : byte
    {
        PING_PONG = 0,
        MESSAGE = 1,
        SUBSCRIBE = 2,
        UNSUBSCRIBE = 3,
        AUTHENTICATE = 4,
        SUBSCRIBE_CONFIRM = 5,
        UNSUBSCRIBE_CONFIRM = 6,
        INSTANCEID_REQUEST = 7
    }
}
