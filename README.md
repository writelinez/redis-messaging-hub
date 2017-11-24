# Redis Messaging Hub

Redis Messaging Hub is a dotnetcore 1.x library that utilizes Redis pub/sub features along with Web Sockets to provide a streamlined and stable messaging system.
Some advantages include the ability to send messages to a client from anywhere by simply publishing to a channel from any location.

The Redis Messaging Hub consists of 2 components. A server component and a client component.
In a typical scenario, the server would be responsible for receiving messages and piping them off to the correct clients.
The client application would send and receive message and also have the ability to join and leave different channels.

All message transmissions are piped through channels which are basically Redis subscriptions. When a client joins a channel a subscription is made in Redis and when a client sends
a message, that message is then published to the designated channel. All transmissions done between the client and the server utilize the Web Socket interface which sits on top
of a simple TCP protocol.

A diagram showing a typical architecture of the messaging system is shown here


![architecture diagram](https://github.com/writelinez/redis-messaging-hub/raw/master/Documentation/howitworks.png "Architecture Diagram")


I’m thinking about possibly standing up some shared servers in the future that would allow the use of the messaging system without needing to setup a server. If I get enough requests, I'll consider
getting something like that going.

### Prerequisites

1. dotnetcore 1.0 - 1.1
2. redis database

### Installing

#### -Server Installation-
Add Nuget package to your dotnetcore project : `Install-Package RedisMessagingHub -Version 0.1.0`

After installing the Nuget Package, you will need to create a hub class. The hub class allows you to intersect the client messages to do further processing. This is also where you would keep track of your users and gate authentication.
The hub class will need to implement the IRedisMessageHub interface. The methods do not need to be utilized but the method stubs will need to be present. An abstract base class is also provided that already implements the IRedisMessageHub. Inheriting this class will provide an easier way to implement the Message hub.

You can use the code snippet below as a baseline.

```C#
using System.Threading.Tasks;
using RedisMessagingHub.Entities;
using RedisMessagingHub.Services;
using RedisMessagingHub.Contracts;

namespace MyProject.Hubs
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
```
 
 After we have the hub class created, we will need to add our configurations and setup our middleware in our Startup.cs file.

 in your appsettings.json file add the following configuration:

 ```Javascript
 {
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },

  //Add this Configuration
  "MessageHubConfig": {
    "RedisConfig": {
      "ConnectionString": "192.168.1.18:6379" //<--- Connection String for your redis database,
      "Database": 1, //<-- Database Instance. Specify -1 to default to the main instance
      "ClearCacheOnStartup": true //<-- Clears the users out of the redis database when the application starts.
    }
  }
}

 ```

 In the ConfigureServices method, add the RedisMessagingHub dependency.
 ```C#
 using RedisMessagingHub.Middleware;
 using MyProject.Hubs;
 namespace WebTest
{
    public class Startup
    {
        ...

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRedisMessagingHub<MessageHub>(options => Configuration.GetSection("MessageHubConfig").Bind(options));

            // Add framework services.
            services.AddMvc();
        }
        ...
 ```

 In the Configure method, add the RedisMessageHub
 ```C#
 public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ...

            app.UseRedisMessagingHub();

            ...
        }
 ```

#### -Client Installation-

The client script wraps Web Sockets under the hood and provides the functionality that is necessary to communicate directly with the server. The client script is currently located in the RedisMessagingHub project under the ClientScript folder. There are both minified and unminified versions. There are plans to add the script as a NPM package in the future, but for now you will need to add the script to the page the old fashioned way.


Copy the redis-message-hub.js or the redis-message-hub.min.js file to your site script directory.


Add the script in a script tag at the bottom of the html page where you want to use it.

```Html
<script src="~/js/redis-message-hub.min.js"></script>
```

You can then access the message hub script object similar to what is show below. The url parameter passed in will point to the server address where you installed the Redis Message Hub on the server.

```Javascript
var hub = new RedisMessageHub({
            url: 'localhost:53308'
        });
```
## Contributing

We would love to get some help on this project. If you wish to contribute to this project, you can either submit your PR's for review or you can contact Chris at chrisbardsley@athosserver.com.


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details