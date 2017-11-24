# Redis Messaging Hub

Redis Messaging Hub is a dotnetcore 1.x library that utilizes Redis pub/sub features along with Web Sockets to provide a streamlined and stable messaging system.
Some advantages include the ability to send messages to a client from anywhere by simply publishing to a channel from any location.

The Redis Messaging Hub consists of 2 components. A server component and a client component.
In a typical scenario, the server would be responsible for receiving messages and piping them off to the correct clients.
The client application would send and receive message and also have the ability to join and leave different channels.

All message transmissions are piped through channels which are basically Redis subscriptions. When a client joins a channel a subscription is made in Redis and when a client sends
a message, that message is then published to the desegnated channel. All transmissions done between the client and the server utilize the Web Socket interface which sits on top
of a simple TCP protocol.

A diagram showing a typical architecture of the messing system is shown here

![architecture diagram](https://github.com/writelinez/redis-messaging-hub/raw/master/Documentation/howitworks.png "Architecture Diagram")


I’m thinking about possibly standing up some shared servers in the future that would allow the use of the messaging system without needing to setup a server. If I get enough requests, I'll consider
getting something like that going.

### Prerequisites

1. dotnetcore 1.0 - 1.1
2. redis database

### Installing

#### -Server Installation-
Add Nuget package to your dotnetcore project : `Install-Package RedisMessagingHub -Version 0.1.0`

After installing the Nuget Package, you will need to create a hub class. The hub class allows you to intersect the client messages to do futher processing. This is also where you would keep track of your users and gate authentication.
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

```
Give the example
```

And repeat

```
until finished
```

End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Explain how to run the automated tests for this system

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Deployment

Add additional notes about how to deploy this on a live system

## Built With



## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Billie Thompson** - *Initial work* - [PurpleBooth](https://github.com/PurpleBooth)

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone who's code was used
* Inspiration
* etc
