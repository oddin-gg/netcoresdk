using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Oddin.OddsFeedSdk.API.Entities;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Abstractions
{
    internal interface IFixtureCache
    {
        LocalizedFixture GetFixture(URN id, CultureInfo culture);
    }
}
