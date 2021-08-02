using System;
using System.Globalization;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface IFixtureCache : IDisposable
    {
        void ClearCacheItem(URN id);

        LocalizedFixture GetFixture(URN id, CultureInfo culture);
    }
}
