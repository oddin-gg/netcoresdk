using System;
using System.Collections.Generic;
using Oddin.OddsFeedSdk.Sessions;
using RabbitMQ.Client.Events;

namespace Oddin.OddsFeedSdk.AMQP.Abstractions;

internal interface IAmqpClient
{
    void Connect(MessageInterest messageInterest, IEnumerable<string> routingKeys);
    void Disconnect();

    event EventHandler<BasicDeliverEventArgs> OnReceived;
}