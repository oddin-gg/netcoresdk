using Microsoft.Extensions.Logging;
using Oddin.OddinSdk.SDK.API;
using Oddin.OddinSdk.SDK.API.Abstractions;
using Oddin.OddinSdk.SDK.Dispatch;
using Oddin.OddinSdk.SDK.Managers;
using Oddin.OddinSdk.SDK.Managers.Abstractions;
using Oddin.OddinSdk.SDK.AMQP;
using Oddin.OddinSdk.SDK.FeedConfiguration;
using System;
using Unity;
using Unity.Injection;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Oddin.OddinSdk.SDK.API.Entities.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Mapping.Abstractions;
using Oddin.OddinSdk.SDK.AMQP.Mapping;
using Oddin.OddinSdk.SDK.Abstractions;
using Oddin.OddinSdk.SDK.Sessions.Abstractions;
using Oddin.OddinSdk.SDK.Sessions;
using System.Collections.Generic;
using Oddin.OddinSdk.Common.Exceptions;
using System.Linq;
using Oddin.OddinSdk.Common;

namespace Oddin.OddinSdk.SDK
{
    public class Feed : DispatcherBase, IOddsFeed
    {
        private readonly IUnityContainer _unityContainer;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOddsFeedConfiguration _config;
        private bool _isOpened;
        private readonly object _isOpenedLock = new object();
        private readonly IList<IOpenable> Sessions = new List<IOpenable>();
        private bool _isDisposed;

        /// <summary>
        /// Gets a <see cref="IProducerManager" /> instance used to retrieve producer related data
        /// </summary>
        /// <value>The producer manager</value>
        public IProducerManager ProducerManager
        {
            get => _unityContainer.Resolve<IProducerManager>();
        }
        
        /// <summary>
        /// Gets a <see cref="IBookmakerDetails"/> instance used to get info about bookmaker and token used
        /// </summary>
        public IBookmakerDetails BookmakerDetails
        {
            get
            {
                try
                {
                    return _unityContainer.Resolve<IApiClient>().GetBookmakerDetails();
                }
                catch (Exception e)
                when (e is CommunicationException
                    || e is MappingException)
                {
                    e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
                }
                return null;
            }
        }


        private void RegisterObjectsToUnityContainer()
        {
            // INFO: registration order matters!

            // register existing logger factory
            _unityContainer.RegisterInstance(typeof(ILoggerFactory), _loggerFactory);
            
            // register ApiClient as singleton
            _unityContainer.RegisterSingleton<IApiClient, ApiClient>(
                new InjectionConstructor(
                    _config,
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );
            
            // register ProducerManager as singleton
            _unityContainer.RegisterSingleton<IProducerManager, ProducerManager>(
                new InjectionConstructor(
                    _unityContainer.Resolve<IApiClient>(),
                    _config.ExceptionHandlingStrategy,
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );

            // register FeedMessageMapper
            _unityContainer.RegisterType<IFeedMessageMapper, FeedMessageMapper>(
                new InjectionConstructor(
                    _unityContainer.Resolve<IApiClient>(),
                    _unityContainer.Resolve<IProducerManager>(),
                    _config.ExceptionHandlingStrategy,
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );

            // register Amqp client as singleton
            _unityContainer.RegisterSingleton<IAmqpClient, AmqpClient>(
                new InjectionConstructor(
                    _config,
                    BookmakerDetails.VirtualHost,
                    (EventHandler<CallbackExceptionEventArgs>)OnAmqpCallbackException,
                    (EventHandler<ShutdownEventArgs>)OnConnectionShutdown,
                    _unityContainer.Resolve<ILoggerFactory>()
                    )
                );
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Feed"/> class
        /// </summary>
        /// <param name="config">Feed configuration</param>
        /// <param name="loggerFactory">Logger factory</param>
        /// <exception cref="ArgumentNullException"/>
        public Feed(IOddsFeedConfiguration config, ILoggerFactory loggerFactory = null) : base(loggerFactory)
        {
            if (config is null)
                throw new ArgumentNullException($"{nameof(config)}");

            _config = config;
            _loggerFactory = loggerFactory;

            _unityContainer = new UnityContainer();
            RegisterObjectsToUnityContainer();
        }

        /// <summary>
        /// Checks if this instance of <see cref="Feed"/> is marked as opened in a thread-safe way
        /// </summary>
        /// <returns><see langword="true"/> if this instance of <see cref="Feed"/> is opened, <see langword="false"/> otherwise</returns>
        private bool IsOpened()
        {
            lock(_isOpenedLock)
            {
                return _isOpened;
            }
        }

        /// <summary>
        /// Checks if this instance of <see cref="Feed"/> is NOT marked as opened and marks it as such in a thread-safe way
        /// </summary>
        /// <returns><see langword="false"/> if this instance of <see cref="Feed"/> was already marked as opened</returns>
        private bool TrySetAsOpened()
        {
            lock(_isOpenedLock)
            {
                if (_isOpened)
                    return false;
                _isOpened = true;
                return true;
            }
        }

        private void SetAsClosed()
        {
            lock(_isOpenedLock)
            {
                _isOpened = false;
            }
        }

        /// <summary>
        /// Opens connection to the feed
        /// </summary>
        /// <exception cref="CommunicationException"/>
        public void Open()
        {
            _log.LogInformation($"Opening {typeof(Feed).Name}...");

            if (TrySetAsOpened() == false)
                throw new InvalidOperationException($"{nameof(Open)} cannot be called when the feed is already opened!");

            try
            {
                foreach (var session in Sessions)
                    session.Open();
            }
            catch (Exception)
            {
                foreach (var openSession in Sessions.Where(s => s.IsOpened()))
                    openSession.Close();

                SetAsClosed();
                throw;
            }
        }

        public void Close()
        {
            _log.LogInformation($"Closing {typeof(Feed)}...");

            foreach (var session in Sessions)
                session.Close();

            SetAsClosed();
        }

        private void InternalDispose(bool disposing)
        {
            if (_isDisposed)
                return;
            
            Close();

            if (disposing)
            {
                try
                {
                    _unityContainer.Dispose();
                }
                catch (Exception ex)
                {
                    _log.LogWarning($"An exception has occurred while disposing the {typeof(Feed).Name} instance. Exception: ", ex);
                }
            }

            _isDisposed = true;
        }

        void IDisposable.Dispose()
        {
            InternalDispose(true);
            GC.SuppressFinalize(this);
        }

        private void OnAmqpCallbackException(object sender, CallbackExceptionEventArgs eventArgs)
        {
            Dispatch(ConnectionException, new ConnectionExceptionEventArgs(eventArgs.Exception, eventArgs.Detail), nameof(ConnectionException));
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            _log.LogWarning($"The AMQP connection was shut down. Cause: {shutdownEventArgs.Cause}");
            
            // TODO: handle feed recovery

            Dispatch(Disconnected, new EventArgs(), nameof(Disconnected));
        }

        /// <summary>
        /// Occurs when an exception occurs in the connection loop
        /// </summary>
        public event EventHandler<ConnectionExceptionEventArgs> ConnectionException;

        /// <summary>
        /// Raised when the current instance of <see cref="IOddsFeed"/> loses connection to the feed
        /// </summary>
        public event EventHandler<EventArgs> Disconnected;

        internal IOddsFeedSession CreateSession(MessageInterest messageInterest)
        {
            if (messageInterest is null)
                throw new ArgumentNullException($"{nameof(messageInterest)}");

            if (IsOpened())
                throw new InvalidOperationException($"Cannot create a session in an already opened feed!");

            var session = new OddsFeedSession(
                _loggerFactory,
                _unityContainer.Resolve<IAmqpClient>(),
                _unityContainer.Resolve<IFeedMessageMapper>(),
                messageInterest,
                _config.ExceptionHandlingStrategy);

            Sessions.Add(session);
            return session;
        }

        public IOddsFeedSessionBuilder CreateBuilder()
        {
            return new OddsFeedSessionBuilder(this);
        }
    }
}
