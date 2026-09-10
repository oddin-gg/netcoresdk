using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.API;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using Oddin.OddsFeedSdk.Configuration.Abstractions;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests.API;

// Side-load delivery is inline on purpose. GetMatchStatus, LoadFixture and the
// schedule warm-up all read the cache straight after their API call returns, so
// moving delivery to a scheduler breaks them silently.
[Collection(CacheSideLoadCollection.Name)]
public class SideLoadDeliveryTests
{
    private static readonly CultureInfo Culture = new("en");

    [Fact]
    public void RestClientDeliversResponsesOnTheCallingThread()
    {
        using var server = StubApiServer.Start("<sports/>");
        using var restClient = new RestClient(new StubConfiguration(server.Authority));

        var observerThread = 0;
        using var subscription = restClient
            .SubscribeForClass<IRequestResult<object>>()
            .Subscribe(_ => observerThread = Environment.CurrentManagedThreadId);

        restClient.SendRequest<SportsModel>("v1/sports/en/sports", HttpMethod.Get, Culture);

        Assert.True(observerThread != 0, "the response was never published");
        Assert.Equal(Environment.CurrentManagedThreadId, observerThread);
    }

    [Fact]
    public void GetMatchStatusReturnsTheStatusItJustFetched()
    {
        var api = DispatchProxy.Create<IApiClient, SummaryApiClientProxy>();
        using var cache = new MatchStatusCache(api);

        Assert.NotNull(cache.GetMatchStatus(new URN("od:match:1")));
    }

    [Fact]
    public void LoadFixtureLeavesTheMatchReadableByPeek()
    {
        var api = DispatchProxy.Create<IApiClient, SummaryApiClientProxy>();
        using var cache = new MatchCache(api);

        var id = new URN("od:match:1");
        cache.LoadFixture(id, Culture);

        Assert.NotNull(cache.PeekMatch(id));
    }

    private sealed class StubApiServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly string _payload;

        private StubApiServer(HttpListener listener, string authority, string payload)
        {
            _listener = listener;
            _payload = payload;
            Authority = authority;
        }

        public string Authority { get; }

        public static StubApiServer Start(string payload)
        {
            var port = FreePort();
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();

            var server = new StubApiServer(listener, $"localhost:{port}", payload);
            Task.Run(server.Serve);
            return server;
        }

        private async Task Serve()
        {
            while (_listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch (Exception)
                {
                    return;
                }

                var body = Encoding.UTF8.GetBytes(_payload);
                context.Response.ContentType = "application/xml";
                context.Response.ContentLength64 = body.Length;
                await context.Response.OutputStream.WriteAsync(body);
                context.Response.Close();
            }
        }

        private static int FreePort()
        {
            var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            var port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();
            return port;
        }

        public void Dispose() => _listener.Close();
    }

    private sealed class StubConfiguration : IFeedConfiguration
    {
        public StubConfiguration(string apiHost) => ApiHost = apiHost;

        public string AccessToken => "token";
        public CultureInfo DefaultLocale => Culture;
        public int MaxInactivitySeconds => 20;
        public int MaxRecoveryExecutionMinutes => 15;
        public int? NodeId => null;
        public ExceptionHandlingStrategy ExceptionHandlingStrategy => ExceptionHandlingStrategy.CATCH;
        public string Host => "localhost";
        public int Port => 5672;
        public bool UseSsl => false;
        public string ApiHost { get; }
        public bool UseApiSsl => false;
        public int HttpClientTimeout => 30;
        public int InitialSnapshotTimeInMinutes => 0;
    }

    // Publishes synchronously before returning, as RestClient does.
    public class SummaryApiClientProxy : DispatchProxy
    {
        private readonly System.Reactive.Subjects.Subject<IRequestResult<object>> _responses = new();

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            switch (targetMethod.Name)
            {
                case nameof(IApiClient.SubscribeForClass):
                    return _responses.OfType<IRequestResult<object>>();
                case nameof(IApiClient.GetMatchSummary):
                    return Publish(Summary());
                case nameof(IApiClient.GetFixture):
                    return Publish(Fixture());
                default:
                    throw new NotSupportedException($"Unexpected API call: {targetMethod.Name}");
            }
        }

        private T Publish<T>(T data)
            where T : class
        {
            _responses.OnNext(RequestResult<object>.Success(
                data,
                HttpStatusCode.OK,
                string.Empty,
                culture: Culture));

            return data;
        }

        private static tournament Tournament() =>
            new()
            {
                id = "od:tournament:1",
                sport = new sport { id = "od:sport:1", name = "Dota 2" }
            };

        private static MatchSummaryModel Summary() =>
            new()
            {
                sport_event = new sportEvent
                {
                    id = "od:match:1",
                    name = "Secret vs Liquid",
                    tournament = Tournament()
                },
                sport_event_status = new sportEventStatus { status = "live", match_status_code = 1 }
            };

        private static FixturesEndpointModel Fixture() =>
            new()
            {
                fixture = new fixture
                {
                    id = "od:match:1",
                    name = "Secret vs Liquid",
                    tournament = Tournament()
                }
            };
    }
}
