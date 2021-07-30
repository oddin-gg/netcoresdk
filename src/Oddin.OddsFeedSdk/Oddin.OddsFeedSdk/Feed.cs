using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.API;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.Dispatch;
using Oddin.OddsFeedSdk.Managers;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using Oddin.OddsFeedSdk.AMQP;
using Oddin.OddsFeedSdk.Configuration;
using System;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Mapping;
using Oddin.OddsFeedSdk.Abstractions;
using Oddin.OddsFeedSdk.Sessions.Abstractions;
using Oddin.OddsFeedSdk.Sessions;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Managers.Recovery;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Oddin.OddsFeedSdk
{
    public class Feed : DispatcherBase, IOddsFeed
    {
        private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(Feed));

        private readonly IServiceProvider _services;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IFeedConfiguration _config;
        private bool _isOpened;
        private readonly object _isOpenedLock = new object();
        private readonly IList<IOpenable> _sessions = new List<IOpenable>();
        private bool _isDisposed;

        public event EventHandler<ConnectionExceptionEventArgs> ConnectionException;
        public event EventHandler<EventArgs> Disconnected;
        public event EventHandler<FeedCloseEventArgs> Closed;
        public event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        private IFeedRecoveryManager _feedRecoveryManager
            => _services.GetService<IFeedRecoveryManager>();

        public IEventRecoveryRequestIssuer EventRecoveryRequestIssuer
            => _services.GetService<IEventRecoveryRequestIssuer>();
    
        public IProducerManager ProducerManager
            => _services.GetService<IProducerManager>();

        public ISportDataProvider SportDataProvider 
            => _services.GetService<ISportDataProvider>();

        public IBookmakerDetails BookmakerDetails
        {
            get
            {
                try
                {
                    return _services.GetService<IApiClient>().GetBookmakerDetails();
                }
                catch (SdkException e)
                {
                    e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
                }
                return null;
            }
        }

        public Feed(IFeedConfiguration config, ILoggerFactory loggerFactory = null)
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));

            _config = config;
            _loggerFactory = loggerFactory;
            SdkLoggerFactory.Initialize(_loggerFactory);

            _services = BuildServices();
        }

        private IServiceProvider BuildServices()
            => new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<IApiModelMapper, ApiModelMapper>()
                .AddSingleton<IFeedMessageMapper, FeedMessageMapper>()
                .AddSingleton<IApiClient, ApiClient>()
                .AddSingleton<IAmqpClient, AmqpClient>()
                .AddSingleton<IProducerManager, ProducerManager>()
                .AddSingleton<IFeedRecoveryManager, FeedRecoveryManager>()
                .AddSingleton<EventHandler<CallbackExceptionEventArgs>>(OnAmqpCallbackException)
                .AddSingleton<EventHandler<ShutdownEventArgs>>(OnConnectionShutdown)
                .AddSingleton<IRequestIdFactory, RequestIdFactory>()
                .AddSingleton<IEventRecoveryRequestIssuer, EventRecoveryRequestIssuer>()
                .AddSingleton<IRestClient, RestClient>()
                .AddSingleton<ISportDataProvider, SportDataProvider>()
                .AddSingleton<ISportDataBuilder, SportDataBuilder>()
                .AddSingleton<IExceptionWrapper, ExceptionWrapper>()
                .AddSingleton<IApiCacheManager, ApiCacheManager>()
                .AddSingleton<ICacheManager, CacheManager>()
                .AddSingleton<ISportDataCache, SportDataCache>()
                .AddSingleton<ITournamentsCache, TournamentsCache>()
                .AddSingleton<ICompetitorCache, CompetitorCache>()
                .AddSingleton<IMatchCache, MatchCache>()
                .AddSingleton<IMatchStatusCache, MatchStatusCache>()
                .AddSingleton<ILocalizedStaticDataCache, LocalizedStaticDataOfMatchStatusCache>()
                .BuildServiceProvider();

        private bool IsOpened()
        {
            lock(_isOpenedLock)
            {
                return _isOpened;
            }
        }

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

        public static ITokenSetter GetConfigurationBuilder()
        {
            return new TokenSetter(new AppConfigurationSectionProvider());
        }
        
        public static IEnumerable<CultureInfo> AvailableLanguages()
        {
            var codes = new[] { "en" };
            return codes
                .Select(c => CultureInfo.GetCultureInfo(c))
                .OrderBy(c => c.Name);
        }

        private void OnEventRecoveryCompleted(object sender, EventRecoveryCompletedEventArgs eventArgs)
        {
            Dispatch(EventRecoveryCompleted, eventArgs, nameof(EventRecoveryCompleted));
        }

        private void OnClosed(object sender, FeedCloseEventArgs eventArgs)
        {
            Dispatch(Closed, eventArgs, nameof(Closed));
        }

        private void OnProducerDown(object sender, ProducerStatusChangeEventArgs eventArgs)
        {
            Dispatch(ProducerDown, eventArgs, nameof(ProducerDown));
        }

        private void OnProducerUp(object sender, ProducerStatusChangeEventArgs eventArgs)
        {
            Dispatch(ProducerUp, eventArgs, nameof(ProducerUp));
        }

        private void AttachToEvents()
        {
            ((IEventRecoveryCompletedDispatcher)EventRecoveryRequestIssuer).EventRecoveryCompleted += OnEventRecoveryCompleted;
            ((FeedRecoveryManager)_feedRecoveryManager).Closed += OnClosed;
            ((FeedRecoveryManager)_feedRecoveryManager).ProducerDown += OnProducerDown;
            ((FeedRecoveryManager)_feedRecoveryManager).ProducerUp += OnProducerUp;
        }

        private void DetachFromEvents()
        {
            ((IEventRecoveryCompletedDispatcher)EventRecoveryRequestIssuer).EventRecoveryCompleted -= OnEventRecoveryCompleted;
            ((FeedRecoveryManager)_feedRecoveryManager).Closed -= OnClosed;
            ((FeedRecoveryManager)_feedRecoveryManager).ProducerDown -= OnProducerDown;
            ((FeedRecoveryManager)_feedRecoveryManager).ProducerUp -= OnProducerUp;
        }

        public void Open()
        {
            _log.LogInformation($"Opening {typeof(Feed).Name}...");

            if (TrySetAsOpened() == false)
                throw new InvalidOperationException($"{nameof(Open)} cannot be called when the feed is already opened!");

            AttachToEvents();
            ProducerManager.Lock();

            try
            {
                foreach (var session in _sessions)
                    session.Open();
            }
            catch (Exception)
            {
                foreach (var openSession in _sessions.Where(s => s.IsOpened()))
                    openSession.Close();

                SetAsClosed();
                throw;
            }

            _feedRecoveryManager.Open();
            ((IEventRecoveryCompletedDispatcher)EventRecoveryRequestIssuer).Open();
        }

        public void Close()
        {
            _log.LogInformation($"Closing {typeof(Feed)}...");

            foreach (var session in _sessions)
                session.Close();

            _feedRecoveryManager.Close();
            ((IEventRecoveryCompletedDispatcher)EventRecoveryRequestIssuer).Close();

            DetachFromEvents();

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
                    foreach (var service in _services.GetServices<IDisposable>())
                        service.Dispose();
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
            
            // INFO: this method is called when Rabbit library informs that the connection has been shut down

            // TODO: handle feed recovery

            Dispatch(Disconnected, new EventArgs(), nameof(Disconnected));
        }

        internal IOddsFeedSession CreateSession(MessageInterest messageInterest)
        {
            if (messageInterest is null)
                throw new ArgumentNullException(nameof(messageInterest));

            if (IsOpened())
                throw new InvalidOperationException($"Cannot create a session in an already opened feed!");

            var session = new OddsFeedSession(
                _services.GetService<IAmqpClient>(),
                _services.GetService<IFeedMessageMapper>(),
                messageInterest,
                _config.ExceptionHandlingStrategy);

            _sessions.Add(session);
            return session;
        }

        public IOddsFeedSessionBuilder CreateBuilder()
            => new OddsFeedSessionBuilder(this);
    }
}
