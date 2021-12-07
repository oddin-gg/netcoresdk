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

        protected readonly IServiceProvider Services;
        private readonly IFeedConfiguration _config;
        private bool _isOpened;
        private readonly object _isOpenedLock = new();
        private readonly IList<IOpenable> _sessions = new List<IOpenable>();
        private bool _isDisposed;

        public event EventHandler<ConnectionExceptionEventArgs> ConnectionException;
        public event EventHandler<EventArgs> Disconnected;
        public event EventHandler<FeedCloseEventArgs> Closed;
        public event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        private IFeedRecoveryManager _feedRecoveryManager
            => Services.GetService<IFeedRecoveryManager>();

        public IEventRecoveryRequestIssuer EventRecoveryRequestIssuer
            => Services.GetService<IEventRecoveryRequestIssuer>();

        public IProducerManager ProducerManager
            => Services.GetService<IProducerManager>();

        public ISportDataProvider SportDataProvider
            => Services.GetService<ISportDataProvider>();

        public IMarketDescriptionManager MarketDescriptionManager
            => Services.GetService<IMarketDescriptionManager>();

        public IBookmakerDetails BookmakerDetails
        {
            get
            {
                try
                {
                    return Services.GetService<IApiClient>().GetBookmakerDetails();
                }
                catch (SdkException e)
                {
                    e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
                }
                return null;
            }
        }

        protected Feed(IFeedConfiguration config, bool isReplay, ILoggerFactory loggerFactory = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            SdkLoggerFactory.Initialize(loggerFactory);

            IExchangeNameProvider exchangeNameProvider = isReplay
                ? (IExchangeNameProvider) new ReplayExchangeNameProvider()
                : (IExchangeNameProvider) new ExchangeNameProvider();

            Services = BuildServices(exchangeNameProvider);
        }

        public Feed(IFeedConfiguration config, ILoggerFactory loggerFactory = null)
            : this(config, false, loggerFactory)
        {
        }

        private IServiceProvider BuildServices(IExchangeNameProvider exchangeNameProvider)
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
                .AddSingleton<ICacheManager, CacheManager>()
                .AddSingleton<ISportDataCache, SportDataCache>()
                .AddSingleton<ITournamentsCache, TournamentsCache>()
                .AddSingleton<ICompetitorCache, CompetitorCache>()
                .AddSingleton<IMatchCache, MatchCache>()
                .AddSingleton<IFixtureCache, FixtureCache>()
                .AddSingleton<IMatchStatusCache, MatchStatusCache>()
                .AddSingleton<ILocalizedStaticDataCache, LocalizedStaticDataOfMatchStatusCache>()
                .AddSingleton<IMarketDescriptionManager, MarketDescriptionManager>()
                .AddSingleton<IMarketDescriptionCache, MarketDescriptionCache>()
                .AddSingleton<IMarketDescriptionFactory, MarketDescriptionFactory>()
                .AddSingleton<IReplayManager, ReplayManager>()
                .AddSingleton(exchangeNameProvider)
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
            var codes = new[] { "en", "br", "de", "es", "fi", "fr", "pl", "pt", "ru", "th", "vi", "zh" };
            return codes.Select(CultureInfo.GetCultureInfo);
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

        private void ValidateMessageInterestsCombination()
        {
            if (_sessions.Count == 0)
                throw new InvalidOperationException("Cannot open the feed when there are no sessions created!");

            if (_sessions.Count == 1)
                return;

            var allMessageInterests = _sessions.Select(s => ((OddsFeedSession)s).MessageInterest.MessageInterestType);
            if (allMessageInterests.Count() != allMessageInterests.Distinct().Count())
                throw new InvalidOperationException("There are duplicate message interests across created sessions!");

            if (allMessageInterests.Contains(MessageInterestType.All))
                throw new InvalidOperationException("AllMessages interest can be used only if it's there is a single session created!");

            var hasPriority = allMessageInterests.Any(mi => mi == MessageInterestType.HighPriority || mi == MessageInterestType.LowPriority);
            var hasMessages = allMessageInterests.Any(mi => mi == MessageInterestType.Prematch || mi == MessageInterestType.Live);
            if (hasPriority && hasMessages)
                throw new InvalidOperationException("Cannot combine priority message interest (high priority / low priority) with other types (prematch / live)!");
        }

        public void Open()
        {
            _log.LogInformation($"Opening {typeof(Feed).Name}...");

            ValidateMessageInterestsCombination();

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

                DetachFromEvents();
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
                    foreach (var service in Services.GetServices<IDisposable>())
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

        // This method is called when Rabbit library informs that the connection has been shut down
        private void OnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            _log.LogWarning($"The AMQP connection was shut down. {shutdownEventArgs.ReplyCode} {shutdownEventArgs.ReplyText} Initiator: {shutdownEventArgs.Initiator} Cause: {shutdownEventArgs.Cause}");

            // Starting feed recovery is not necessary because if no alive message is received recovery is started anyway.

            if (shutdownEventArgs.Initiator == ShutdownInitiator.Application)
                Dispatch(Disconnected, EventArgs.Empty, nameof(Disconnected));
            else
                Dispatch(Closed, new FeedCloseEventArgs($"{shutdownEventArgs.ReplyCode} {shutdownEventArgs.ReplyText}"), nameof(Closed));
        }

        internal IOddsFeedSession CreateSession(MessageInterest messageInterest)
        {
            if (messageInterest is null)
                throw new ArgumentNullException(nameof(messageInterest));

            if (IsOpened())
                throw new InvalidOperationException($"Cannot create a session in an already opened feed!");

            var session = new OddsFeedSession(
                this,
                Services.GetService<IAmqpClient>(),
                Services.GetService<IFeedMessageMapper>(),
                messageInterest,
                _config);

            _sessions.Add(session);
            return session;
        }

        public IOddsFeedSessionBuilder CreateBuilder()
            => new OddsFeedSessionBuilder(this);
    }
}
