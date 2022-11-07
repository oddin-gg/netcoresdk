using System;
using System.Globalization;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface ITvChannel
{
    string Name { get; }

    DateTime? StartTime { get; }

    string StreamUrl { get; }

    CultureInfo Language { get; }
}