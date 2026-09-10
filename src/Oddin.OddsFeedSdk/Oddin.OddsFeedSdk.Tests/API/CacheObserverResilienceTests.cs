using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Oddin.OddsFeedSdk.API;
using Oddin.OddsFeedSdk.API.Abstractions;
using Oddin.OddsFeedSdk.API.Models;
using Oddin.OddsFeedSdk.Common;
using Xunit;

namespace Oddin.OddsFeedSdk.Tests.API;

// An exception escaping a side-load observer is not a one-off: Rx disposes the
// subscription permanently, so the cache silently stops taking API data for the
// rest of the process, and Subject rethrows it into whoever made the API call.
[Collection(CacheSideLoadCollection.Name)]
public class CacheObserverResilienceTests
{
    private static readonly CultureInfo Culture = new("en");

    private static URN PlayerId => new("od:player:1");

    [Fact]
    public void PlayerCacheKeepsSideLoadingAfterAResponseThatThrows()
    {
        var api = DispatchProxy.Create<IApiClient, PublishOnlyApiClientProxy>();
        var proxy = (PublishOnlyApiClientProxy)api;

        using var playerCache = new PlayerCache(api);

        // players is null, so the observer's switch arm calls .ToArray() on null.
        var escapedIntoCaller = false;
        try
        {
            proxy.Publish(new competitorProfileEndpoint { players = null }, Culture);
        }
        catch
        {
            escapedIntoCaller = true;
        }

        proxy.Publish(
            new competitorProfileEndpoint
            {
                players = new List<player_profilePlayer>
                {
                    new() { id = "od:player:1", name = "Dendi", full_name = "Danil Ishutin" }
                }
            },
            Culture);

        Assert.False(
            escapedIntoCaller,
            "a bad side-load response escaped into the API caller, which sees it as a failed request");

        // GetPlayerProfile throws in this proxy, so a non-null result can only have come
        // from the observer — proving the subscription survived the first response.
        Assert.NotNull(playerCache.GetPlayer(PlayerId, new[] { Culture }));
    }

    // Publishes on demand; every direct API call fails, so anything found in a cache
    // must have been side-loaded.
    public class PublishOnlyApiClientProxy : DispatchProxy
    {
        private readonly Subject<IRequestResult<object>> _responses = new();

        public void Publish(object data, CultureInfo culture) =>
            _responses.OnNext(RequestResult<object>.Success(
                data,
                HttpStatusCode.OK,
                string.Empty,
                culture: culture));

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod.Name == nameof(IApiClient.SubscribeForClass))
                return _responses.OfType<IRequestResult<object>>();

            throw new InvalidOperationException($"API unavailable: {targetMethod.Name}");
        }
    }
}
