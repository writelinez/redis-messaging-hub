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

A step by step series of examples that tell you have to get a development env running

Say what the step will be

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
