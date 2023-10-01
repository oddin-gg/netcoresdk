using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oddin.OddsFeedSdk.Abstractions;
using Oddin.OddsFeedSdk.AMQP;
using Oddin.OddsFeedSdk.AMQP.Abstractions;
using Oddin.OddsFeedSdk.AMQP.Mapping;
using Oddin.OddsFeedSdk.AMQP.Mapping.Abstractions;
using Oddin.OddsFeedSdk.API;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Oddin.OddsFeedSdk.Dispatch;
using Oddin.OddsFeedSdk.Dispatch.EventArguments;
using Oddin.OddsFeedSdk.Exceptions;
using Oddin.OddsFeedSdk.Managers;
using Oddin.OddsFeedSdk.Managers.Abstractions;
using Oddin.OddsFeedSdk.Managers.Recovery;
using Oddin.OddsFeedSdk.Sessions;
using Oddin.OddsFeedSdk.Sessions.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Oddin.OddsFeedSdk;

public class Feed : DispatcherBase, IOddsFeed
{
    private const string SNAPSHOT_COMPLETE_ROUTING_KEY_TEMPLATE = "-.-.-.snapshot_complete.-.-.-.{0}";

    private static readonly ILogger _log = SdkLoggerFactory.GetLogger(typeof(Feed));
    private readonly IFeedConfiguration _config;
    private readonly object _isOpenedLock = new();
    private readonly IList<OddsFeedSession> _sessions = new List<OddsFeedSession>();

    protected readonly IServiceProvider Services;
    private bool _isDisposed;
    private bool _isOpened;
    private OddsFeedSession _possibleAliveSession;

    protected Feed(IFeedConfiguration config, bool isReplay, ILoggerFactory loggerFactory = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        SdkLoggerFactory.Initialize(loggerFactory);

        var exchangeNameProvider = isReplay
            ? (IExchangeNameProvider)new ReplayExchangeNameProvider()
            : new ExchangeNameProvider();

        Services = BuildServices(exchangeNameProvider);
    }

    public Feed(IFeedConfiguration config, ILoggerFactory loggerFactory = null)
        : this(config, false, loggerFactory)
    {
    }

    private ISdkRecoveryManager _recoveryManager =>
        Services.GetService<ISdkRecoveryManager>() ?? throw new NullReferenceException();

    private IApiClient _apiClient => Services.GetService<IApiClient>() ?? throw new NullReferenceException();

    private CacheManager _cacheManager =>
        (CacheManager)Services.GetService<ICacheManager>() ?? throw new NullReferenceException();

    public event EventHandler<ConnectionExceptionEventArgs> ConnectionException;
    public event EventHandler<EventArgs> Disconnected;
    public event EventHandler<FeedCloseEventArgs> Closed;
    public event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;
    public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;
    public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

    public IRecoveryManager RecoveryManager =>
        Services.GetService<ISdkRecoveryManager>() ?? throw new NullReferenceException();

    public IProducerManager ProducerManager =>
        Services.GetService<IProducerManager>() ?? throw new NullReferenceException();

    public ISportDataProvider SportDataProvider =>
        Services.GetService<ISportDataProvider>() ?? throw new NullReferenceException();

    public IMarketDescriptionManager MarketDescriptionManager =>
        Services.GetService<IMarketDescriptionManager>() ?? throw new NullReferenceException();

    public IBookmakerDetails BookmakerDetails
    {
        get
        {
            try
            {
                return _apiClient.GetBookmakerDetails();
            }
            catch (SdkException e)
            {
                e.HandleAccordingToStrategy(GetType().Name, _log, _config.ExceptionHandlingStrategy);
            }

            return null;
        }
    }

    public void Open()
    {
        if (TrySetAsOpened() == false)
            throw new InvalidOperationException($"{nameof(Open)} cannot be called when the feed is already opened!");

        if (!_sessions.Any())
        {
            throw new InvalidOperationException("Feed created without sessions");
        }

        _log.LogInformation($"Opening {nameof(Feed)}...");

        var availableProducers = ProducerManager.Producers;
        var requestedProducers = new HashSet<int>();
        foreach (var s in _sessions)
        {
            var producers = s.MessageInterest.GetPossibleSourceProducers(availableProducers);
            requestedProducers.UnionWith(producers);
        }

        foreach (var producer in availableProducers)
        {
            if (!requestedProducers.Contains(producer.Id))
            {
                var p = (Producer)ProducerManager.Get(producer.Id);
                p.ProducerData.Enabled = false;
            }
        }

        var sessionRoutingKeys = GenerateKeys(_sessions, _config);

        var hasReplay = _sessions.Any(s => s.Feed is ReplayFeed);
        var replayOnly = hasReplay && _sessions.Count == 1;
        var hasAliveMessageInterest = _sessions.Any(s =>
            s.MessageInterest.MessageInterestType == MessageInterest.SystemAliveOnlyMessages.MessageInterestType);

        if (!hasAliveMessageInterest && !replayOnly)
        {
            _possibleAliveSession = NewSession(MessageInterest.SystemAliveOnlyMessages);
            _possibleAliveSession.Open(MessageInterest.SystemAliveOnlyMessages.RoutingKeys);
        }

        AttachToEvents();
        ProducerManager.Lock();

        try
        {
            foreach (var session in _sessions)
            {
                var found = sessionRoutingKeys.TryGetValue(session.SessionId, out var routingKeys);
                if (!found) throw new InvalidOperationException("Missing routing keys for session");
                session.Open(routingKeys);
            }
        }
        catch (Exception)
        {
            foreach (var openSession in _sessions.Where(s => s.IsOpened()))
                openSession.Close();

            DetachFromEvents();
            SetAsClosed();
            throw;
        }

        _recoveryManager.Open(replayOnly);
    }

    public void Close()
    {
        _log.LogInformation($"Closing {typeof(Feed)}...");

        foreach (var session in _sessions)
            session.Close();

        ( (RecoveryManager)_recoveryManager ).Close();

        DetachFromEvents();
        SetAsClosed();
    }

    void IDisposable.Dispose()
    {
        InternalDispose(true);
        GC.SuppressFinalize(this);
    }

    public IOddsFeedSessionBuilder CreateBuilder()
        => new OddsFeedSessionBuilder(this);

    private IServiceProvider BuildServices(IExchangeNameProvider exchangeNameProvider)
        => new ServiceCollection()
            .AddSingleton(_config)
            .AddSingleton<IApiModelMapper, ApiModelMapper>()
            .AddSingleton<IFeedMessageMapper, FeedMessageMapper>()
            .AddSingleton<IApiClient, ApiClient>()
            .AddSingleton<IProducerManager, ProducerManager>()
            .AddSingleton<IRequestIdFactory, RequestIdFactory>()
            .AddSingleton<ISdkRecoveryManager, RecoveryManager>()
            .AddSingleton<IRestClient, RestClient>()
            .AddSingleton<ISportDataProvider, SportDataProvider>()
            .AddSingleton<ISportDataBuilder, SportDataBuilder>()
            .AddSingleton<IExceptionWrapper, ExceptionWrapper>()
            .AddSingleton<ICacheManager, CacheManager>()
            .AddSingleton<ISportDataCache, SportDataCache>()
            .AddSingleton<ITournamentsCache, TournamentsCache>()
            .AddSingleton<ICompetitorCache, CompetitorCache>()
            .AddSingleton<IPlayerCache, PlayerCache>()
            .AddSingleton<IMatchCache, MatchCache>()
            .AddSingleton<IFixtureCache, FixtureCache>()
            .AddSingleton<IMatchStatusCache, MatchStatusCache>()
            .AddSingleton<ILocalizedStaticDataCache, LocalizedStaticDataOfMatchStatusCache>()
            .AddSingleton<IMarketDescriptionManager, MarketDescriptionManager>()
            .AddSingleton<IMarketDescriptionCache, MarketDescriptionCache>()
            .AddSingleton<IMarketDescriptionFactory, MarketDescriptionFactory>()
            .AddSingleton<IReplayManager, ReplayManager>()
            .AddSingleton<IMarketVoidReasonsCache, MarketVoidReasonsCache>()
            .AddSingleton(exchangeNameProvider)
            .BuildServiceProvider();

    private bool IsOpened()
    {
        lock (_isOpenedLock)
        {
            return _isOpened;
        }
    }

    private bool TrySetAsOpened()
    {
        lock (_isOpenedLock)
        {
            if (_isOpened)
                return false;

            _isOpened = true;
            return true;
        }
    }

    private void SetAsClosed()
    {
        lock (_isOpenedLock)
        {
            _isOpened = false;
        }
    }

    public static ITokenSetter GetConfigurationBuilder() => new TokenSetter(new AppConfigurationSectionProvider());

    public static IEnumerable<CultureInfo> AvailableLanguages()
    {
        var codes = new[] { "en", "br", "de", "es", "fi", "fr", "pl", "pt", "ru", "th", "vi", "zh" };
        return codes.Select(CultureInfo.GetCultureInfo);
    }

    private void OnEventRecoveryCompleted(object sender, EventRecoveryCompletedEventArgs eventArgs) =>
        Dispatch(EventRecoveryCompleted, eventArgs, nameof(EventRecoveryCompleted));

    private void OnProducerDown(object sender, ProducerStatusChangeEventArgs eventArgs) =>
        Dispatch(ProducerDown, eventArgs, nameof(ProducerDown));

    private void OnProducerUp(object sender, ProducerStatusChangeEventArgs eventArgs) =>
        Dispatch(ProducerUp, eventArgs, nameof(ProducerUp));

    private void AttachToEvents()
    {
        ( (RecoveryManager)_recoveryManager ).EventProducerDown += OnProducerDown;
        ( (RecoveryManager)_recoveryManager ).EventProducerUp += OnProducerUp;
        ( (RecoveryManager)_recoveryManager ).EventRecoveryCompleted += OnEventRecoveryCompleted;
    }

    private void DetachFromEvents()
    {
        ( (RecoveryManager)_recoveryManager ).EventRecoveryCompleted -= OnEventRecoveryCompleted;
        ( (RecoveryManager)_recoveryManager ).EventProducerDown -= OnProducerDown;
        ( (RecoveryManager)_recoveryManager ).EventProducerUp -= OnProducerUp;
    }

    private void ValidateMessageInterestsCombination()
    {
        switch (_sessions.Count)
        {
            case 0:
                throw new InvalidOperationException("Cannot open the feed when there are no sessions created!");
            case 1:
                return;
        }

        var allMessageInterests = _sessions.Select(s => s.MessageInterest.MessageInterestType).ToList();
        if (allMessageInterests.Count() != allMessageInterests.Distinct().Count())
            throw new InvalidOperationException("There are duplicate message interests across created sessions!");

        if (allMessageInterests.Contains(MessageInterestType.All))
            throw new InvalidOperationException(
                "AllMessages interest can be used only if it's there is a single session created!");

        var hasPriority =
            allMessageInterests.Any(mi => mi is MessageInterestType.HighPriority or MessageInterestType.LowPriority);
        var hasMessages = allMessageInterests.Any(mi => mi is MessageInterestType.Prematch or MessageInterestType.Live);
        if (hasPriority && hasMessages)
            throw new InvalidOperationException(
                "Cannot combine priority message interest (high priority / low priority) with other types (prematch / live)!");
    }

    private IDictionary<Guid, IEnumerable<string>> GenerateKeys(IList<OddsFeedSession> sessions,
        IFeedConfiguration config)
    {
        ValidateMessageInterestsCombination();

        var bothLowAndHigh = sessions.Count(s =>
                                 s.MessageInterest.MessageInterestType is
                                     MessageInterestType.LowPriority or MessageInterestType.HighPriority
                             )
                             == 2;

        var snapshotRoutingKey =
            string.Format(SNAPSHOT_COMPLETE_ROUTING_KEY_TEMPLATE, config.NodeId?.ToString() ?? "-");

        var result = new Dictionary<Guid, IEnumerable<string>>();
        foreach (var session in sessions)
        {
            var sessionRoutingKeys = new List<string>();

            var basicRoutingKeys = session.MessageInterest.RoutingKeys.ToList();

            foreach (var key in basicRoutingKeys)
            {
                string basicRoutingKey;
                if (config.NodeId != null)
                {
                    sessionRoutingKeys.Add($"{key}.{config.NodeId?.ToString()}.#");
                    basicRoutingKey = $"{key}.-.#";
                }
                else
                {
                    basicRoutingKey = $"{key}.#";
                }

                if (bothLowAndHigh &&
                    session.MessageInterest.MessageInterestType == MessageInterestType.LowPriority)
                {
                    sessionRoutingKeys.Add(basicRoutingKey);
                }
                else
                {
                    sessionRoutingKeys.Add(snapshotRoutingKey);
                    sessionRoutingKeys.Add(basicRoutingKey);
                }
            }

            if (session.MessageInterest.MessageInterestType != MessageInterestType.SystemAlive)
            {
                sessionRoutingKeys.AddRange(MessageInterest.SystemAliveOnlyMessages.RoutingKeys);
            }

            result[session.SessionId] = sessionRoutingKeys.Distinct();
        }

        return result;
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
                _log.LogWarning($"An exception has occurred while disposing the {nameof(Feed)} instance. Exception: ",
                    ex);
            }
        }

        _isDisposed = true;
    }

    private void OnAmqpCallbackException(object sender, CallbackExceptionEventArgs eventArgs) => Dispatch(
        ConnectionException, new ConnectionExceptionEventArgs(eventArgs.Exception, eventArgs.Detail),
        nameof(ConnectionException));

    // This method is called when Rabbit library informs that the connection has been shut down
    private void OnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
    {
        _log.LogWarning(
            $"The AMQP connection was shut down. {shutdownEventArgs.ReplyCode} {shutdownEventArgs.ReplyText} Initiator: {shutdownEventArgs.Initiator} Cause: {shutdownEventArgs.Cause}");

        // Starting feed recovery is not necessary because if no alive message is received recovery is started anyway.

        if (shutdownEventArgs.Initiator == ShutdownInitiator.Application)
            Dispatch(Disconnected, EventArgs.Empty, nameof(Disconnected));
        else
            Dispatch(Closed, new FeedCloseEventArgs($"{shutdownEventArgs.ReplyCode} {shutdownEventArgs.ReplyText}"),
                nameof(Closed));
    }

    internal IOddsFeedSession BuildSession(MessageInterest messageInterest)
    {
        if (messageInterest is null)
            throw new ArgumentNullException(nameof(messageInterest));

        if (IsOpened())
            throw new InvalidOperationException("Cannot create a session in an already opened feed!");

        var session = NewSession(messageInterest);
        _sessions.Add(session);
        return session;
    }

    private OddsFeedSession NewSession(MessageInterest messageInterest) =>
        new(
            this,
            _config,
            Services.GetService<IFeedMessageMapper>(),
            messageInterest,
            ProducerManager,
            (RecoveryManager)_recoveryManager,
            _cacheManager,
            Services.GetService<IApiClient>(),
            OnAmqpCallbackException,
            OnConnectionShutdown,
            Services.GetService<IExchangeNameProvider>()
        );
}