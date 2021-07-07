using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.EventArguments;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch;
using Oddin.OddinSdk.SDK.Sessions.Abstractions;
using System;

namespace Oddin.OddinSdk.SDK.Sessions
{
    internal class OddsFeedSession : DispatcherBase, IOddsFeedSession
    {
        private readonly IAmqpClient _amqpClient;
        private readonly MessageInterest _messageInterest;

        public string Name { get; }

        public OddsFeedSession(
            ILoggerFactory loggerFactory,
            IAmqpClient amqpClient,
            MessageInterest messageInterest)
            : base(loggerFactory)
        {
            _amqpClient = amqpClient;
            _messageInterest = messageInterest;

            Name = messageInterest.Name;
        }

        public event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;
        public event EventHandler<OddsChangeEventArgs<ISportEvent>> OnOddsChange;

        public void Open()
        {
            // attach amqp client events

            _amqpClient.Connect(_messageInterest);

            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}
