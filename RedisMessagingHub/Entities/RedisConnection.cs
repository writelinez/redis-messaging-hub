using System;
using System.Collections.Generic;
using System.Text;

namespace RedisMessagingHub.Entities
{
    public class RedisConnection
    {
        public Guid Id { get; set; }

        public List<ConnectionChannel> Channels { get; set; } = new List<ConnectionChannel>();

        public RedisConnection(Guid id)
        {
            Id = id;
        }
    }

    public class ConnectionChannel
    {
        public string Name { get; set; } = "";
    }
}
