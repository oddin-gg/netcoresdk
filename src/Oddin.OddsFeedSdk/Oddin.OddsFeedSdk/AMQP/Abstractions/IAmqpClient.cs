using Oddin.OddsFeedSdk.AMQP.EventArguments;
using Oddin.OddsFeedSdk.AMQP.Messages;
using Oddin.OddsFeedSdk.Sessions;
using System;
using System.Collections.Generic;
using RabbitMQ.Client.Events;

namespace Oddin.OddsFeedSdk.AMQP.Abstractions
{
    internal interface IAmqpClient
    {
        void Connect(MessageInterest messageInterest, IEnumerable<string> routingKeys);
        void Disconnect();

        event EventHandler<BasicDeliverEventArgs> OnReceived;
    }
}