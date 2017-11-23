using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RedisMessagingHub.Config;
using RedisMessagingHub.Entities;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedisMessagingHub.Services
{
    public sealed class RedisService
    {
        private static volatile ConnectionMultiplexer _redis = null;
        private static object syncRoot = new object();

        private readonly RedisConfiguration _config = null;

        public RedisService(IOptions<RedisConfiguration> config)
        {
            _config = config?.Value;

            if (_redis == null)
            {
                lock (syncRoot)
                {
                    if (_redis == null)
                    {
                        _redis = ConnectionMultiplexer.Connect(config?.Value?.ConnectionString);
                        if (config.Value.ClearCacheOnStartup)
                            ClearAllUserConnections().GetAwaiter().GetResult();
                    }
                }
            }
        }

        public ConnectionMultiplexer Connection => _redis;

        public IDatabase Database => _redis.GetDatabase(_config.Database);

        public ISubscriber Subscriber => _redis.GetSubscriber();

        public IServer Server => _redis.GetServer(_redis.GetEndPoints().First());

        public async Task ClearAllUserConnections()
        {
            foreach (var key in Server.Keys(pattern: "connection:*"))
            {
                await _redis.GetDatabase().KeyDeleteAsync(key);
            }
        }

        public async Task<IQueryable<RedisConnection>> GetAllUserConnections()
        {
            ICollection<RedisConnection> connections = new HashSet<RedisConnection>();

            foreach (var key in Server.Keys(pattern: "connection:*"))
            {
                string raw = await Database.StringGetAsync(key);
                RedisConnection conn = JsonConvert.DeserializeObject<RedisConnection>(raw);
                connections.Add(conn);
            }

            return connections.AsQueryable();
        }

        public async Task<RedisConnection> GetUserConnection(string instanceId)
        {
            RedisConnection conn = null;

            string key = $"connection:{instanceId}";
            string raw = await Database.StringGetAsync(key);
            if (!string.IsNullOrEmpty(raw))
            {
                conn = JsonConvert.DeserializeObject<RedisConnection>(raw);
            }

            return conn;
        }
    }
}
