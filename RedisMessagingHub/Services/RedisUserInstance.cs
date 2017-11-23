using Newtonsoft.Json;
using RedisMessagingHub.Entities;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisMessagingHub.Services
{
    public class RedisUserInstance : IDisposable
    {
        private readonly RedisService _redis = null;
        private readonly Guid _instanceId = Guid.Empty;
        private readonly Dictionary<string, Action<RedisChannel, RedisValue>> _channelDelegates = new Dictionary<string, Action<RedisChannel, RedisValue>>();

        public RedisUserInstance(RedisService redis)
        {
            _redis = redis;

            _instanceId = Guid.NewGuid();

            SetConnection()
                .GetAwaiter()
                .GetResult();
        }

        internal async Task SetConnection()
        {
            string key = $"connection:{_instanceId}";
            string value = JsonConvert.SerializeObject(new RedisConnection(_instanceId));
            await _redis.Database.StringSetAsync(key, value);
        }

        internal async Task SetConnection(RedisConnection data)
        {
            string key = $"connection:{_instanceId}";
            string value = JsonConvert.SerializeObject(data);
            await _redis.Database.StringSetAsync(key, value);
        }

        internal async Task KillConnection()
        {
            string key = $"connection:{_instanceId}";
            await _redis.Database.KeyDeleteAsync(key);
        }

        public async Task<RedisConnection> GetConnection()
        {
            RedisConnection connection = null;
            string key = $"connection:{_instanceId}";

            string rawValue = await _redis.Database.StringGetAsync(key);
            if (!string.IsNullOrEmpty(rawValue))
            {
                connection = JsonConvert.DeserializeObject<RedisConnection>(rawValue);
            }
            return connection;
        }

        public async Task PublishToChannel(string channel, string message)
        {
            await _redis.Subscriber.PublishAsync(channel, message);
        }

        public async Task SubscribeToChannel(string channel, Action<RedisChannel, RedisValue> recieved)
        {
            //IF NO SUBSCRIPTION DELEGATE EXISTS
            if (!_channelDelegates.ContainsKey(channel))
            {
                //IF CONNECTION IS IN DATABASE
                RedisConnection conn = await GetConnection();
                if (conn != null)
                {
                    //ADD SUBSCRIPTION DELEGATE
                    _channelDelegates.Add(channel, (chn, msg) =>
                    {
                        recieved(chn, msg);
                    });

                    //IF CHANNEL DOES NOT EXIST FOR CONNECTION, ADD IT
                    if (!conn.Channels.Any(t => t.Name.Equals(channel, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        conn.Channels.Add(new ConnectionChannel() { Name = channel });
                        await SetConnection(conn);
                    }

                    //SUBSCRIBE TO CHANNEL
                    await _redis.Subscriber.SubscribeAsync(channel, _channelDelegates[channel]);
                }
            }
        }

        public async Task UnsubscribeFromChannel(string channel)
        {
            //IF A SUBSCRIPTION DELEGATE EXISTS
            if (_channelDelegates.ContainsKey(channel))
            {
                //UNSUBSCRIBE FROM CHANNEL
                await _redis.Subscriber.UnsubscribeAsync(channel, _channelDelegates[channel]);

                //REMOVE SUBSCRIPTION DELEGATE
                _channelDelegates[channel] = null;
                _channelDelegates.Remove(channel);

                //REMOVE THE CHANNEL FROM THE DB IF IT EXISTS.
                RedisConnection conn = await GetConnection();
                if (conn != null)
                {
                    ConnectionChannel fCh = conn.Channels.SingleOrDefault(t => t.Name.Equals(channel, StringComparison.CurrentCultureIgnoreCase));
                    if (fCh != null)
                    {
                        conn.Channels.Remove(fCh);
                        await SetConnection(conn);
                    }
                }
            }
        }

        #region Disposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        ~RedisUserInstance()
        {
            Dispose(false);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources  
                RedisConnection conn = GetConnection().GetAwaiter().GetResult();
                if (conn != null)
                {
                    if (conn.Channels != null)
                    {
                        foreach (var channel in conn.Channels)
                        {
                            UnsubscribeFromChannel(channel.Name).GetAwaiter().GetResult();
                        }
                    }

                    KillConnection().GetAwaiter().GetResult();
                    conn = null;
                }
            }         
        }
        #endregion
    }
}
