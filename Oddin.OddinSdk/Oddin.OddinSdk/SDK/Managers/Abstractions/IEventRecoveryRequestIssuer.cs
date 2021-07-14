﻿using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch.EventArguments;
using System;
using System.Threading.Tasks;

namespace Oddin.OddinSdk.SDK.Managers.Abstractions
{
    /// <summary>
    /// Defines a contract implemented by classes used to issue event message recovery requests to the feed
    /// </summary>
    public interface IEventRecoveryRequestIssuer
    {
        /// <summary>
        /// Asynchronously requests messages for the specified sport event and returns a request number used when issuing the request
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery</param>
        /// <param name="eventId">A <see cref="URN"/> specifying the sport event for which to request the messages</param>
        /// <returns> <see cref="Task{HttpStatusCode}"/> representing the async operation</returns>
        Task<long> RecoverEventMessagesAsync(IProducer producer, URN eventId);

        /// <summary>
        /// Asynchronously requests stateful messages for the specified sport event and returns a request number used when issuing the request
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery</param>
        /// <param name="eventId">A <see cref="URN"/> specifying the sport event for which to request the messages</param>
        /// <returns> <see cref="Task{HttpStatusCode}"/> representing the async operation</returns>
        Task<long> RecoverEventStatefulMessagesAsync(IProducer producer, URN eventId);
    }
}
