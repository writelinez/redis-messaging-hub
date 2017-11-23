using RedisMessagingHub.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisMessagingHub.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public MessageType Type { get; set; }

        public string Channel { get; set; }

        public object Data { get; set; }

        public DateTime Date { get; set; }
    }
}
