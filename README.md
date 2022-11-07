.NET SDK
----------------

Purpose of this SDK is to make integration process much smoother and easier. This SDK should take care of all
connection,
data binding and other issues related to connection to API and Feed.

### How to start

```c#
// Build configuration
var config = Feed
    .GetConfigurationBuilder()
    .SetAccessToken(TOKEN)
    .SelectIntegration()
    .Build();

// Create Feed
var feed = new Feed(config, you_logger_factory);

// Subscribe to messages
var session = feed
    .CreateBuilder()
    .SetMessageInterest(MessageInterest.AllMessages)
    .Build();

// Subscribe to Feed events 
feed.EventRecoveryCompleted += OnEventRecoveryComplete; // Your methods
feed.ProducerDown += OnProducerDown;
feed.ProducerUp += OnProducerUp;
feed.ConnectionException += OnConnectionException;
feed.Disconnected += OnDisconnected;
feed.Closed += OnClosed;

// Subscribe to Session events
session.OnOddsChange += OnOddsChangeReceived; // Your methods
session.OnBetStop += OnBetStopReceived;
session.OnBetSettlement += OnBetSettlement;
session.OnRollbackBetSettlement += OnRollbackBetSettlement;
session.OnRollbackBetCancel += OnRollbackBetCancel;
session.OnUnparsableMessageReceived += OnUnparsableMessageReceived;
session.OnBetCancel += Session_OnBetCancel;
session.OnFixtureChange += Session_OnFixtureChange;

// Open connection
feed.Open();

// Do your work...

// Close at the end
feed.Close();
```

You are all set and messages should start coming.

You can check more information via appropriate managers - SportsInfoManager, MarketDescriptionManager, ReplayManager and
others
For example:

```c#
var sportsInfoManager = feed.SportDataProvider;

// Fetch all sports with default locale
var sports = await sportsInfoManager.GetSportsAsync();

// Fetch all active tournaments with default locale
var tournaments = sportsInfoManager.GetActiveTournaments("Dota 2");
```

### Replay

You can use replay feature to receive data from previously played events. You need to build a replay session via session
builder, add events to replay list and play it.

```c#
// Set up your odds feed config
var config = Feed
    .GetConfigurationBuilder()
    .SetAccessToken(TOKEN)
    .SelectReplay()
    .SetNodeId(1)
    .Build();

// Build replay session
var feed = new ReplayFeed(config, you_logger_factory);

var session = feed
    .CreateBuilder()
    .SetMessageInterest(MessageInterest.AllMessages)
    .Build();

// Here subscribe to events
session.OnOddsChange += OnOddsChangeReceived;
...

// Open connection
feed.Open();

// Work with replay feature:
var replayManager = feed.ReplayManager;

var match1 = new URN("od:match:1");
var match2 = new URN("od:match:2");

// Stop replay
await replayManager.StopReplay();

// Get URNs of events already in queue
var eventsInQueue = await replayManager.GetEventsInQueue();

//Get list of events as ISportEvents 
var replayList = await replayManager.GetReplayList();

// If match1 is not in queue add it
if (eventsInQueue.Any(q => q == match1) == false)
    await replayManager.AddMessagesToReplayQueue(match1);

// If match2 is not in queue add it
if (eventsInQueue.Any(q => q == match2) == false)
    await replayManager.AddMessagesToReplayQueue(match2);

// Start the replay
var result = await replayManager.StartReplay(30, 500);

// Do you work...

// Clear replay at the end
await replayManager.StopAndClearReplay();

// Close at the end
feed.Close();

```

You should start receiving event odds via provided listener.

 
