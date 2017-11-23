using System;
using System.Collections.Generic;
using System.Text;

namespace RedisMessagingHub.Config
{
    public class RedisConfiguration
    {
        public string ConnectionString { get; set; }

        public int Database { get; set; } = -1;

        public bool ClearCacheOnStartup { get; set; } = true;
    }
}
