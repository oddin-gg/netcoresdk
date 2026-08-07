using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.Sessions;
using RabbitMQ.Client.Events;

namespace Oddin.OddsFeedSdk.AMQP.Abstractions;

internal interface IAmqpClient
{
    Task Connect(MessageInterest messageInterest, IEnumerable<string> routingKeys);
    Task Disconnect();

    event AsyncEventHandler<BasicDeliverEventArgs> OnReceived;
}