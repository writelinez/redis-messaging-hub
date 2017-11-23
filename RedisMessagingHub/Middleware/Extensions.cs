using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RedisMessagingHub.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using RedisMessagingHub.Config;

namespace RedisMessagingHub.Middleware
{
    public static class Extensions
    {
        public static IServiceCollection AddRedisMessagingHub<THub>(this IServiceCollection services, Action<RedisMessagingHubConfiguration> configureOptions) where THub : class, IRedisMessageHub
        {
            //Register Services
            services.AddSingleton<RedisService>();
            services.AddScoped<IRedisMessageHub, THub>();
            services.AddScoped<WebSocketService>();

            RedisMessagingHubConfiguration config = new RedisMessagingHubConfiguration();
            configureOptions(config);

            //Configure Services
            services.Configure<RedisConfiguration>(options => options.ConnectionString = config.RedisConfig.ConnectionString);

            return services;
        }

        public static IApplicationBuilder UseRedisMessagingHub(this IApplicationBuilder app)
        {
            
            app.UseWebSockets();

            return app.Use(async (context, next) => 
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocketService webSocketService = context.RequestServices.GetService<WebSocketService>();

                    await webSocketService.StartSocketListener(context);
                }
                else
                {
                    await next.Invoke();
                }
            });
        }
    }
}
