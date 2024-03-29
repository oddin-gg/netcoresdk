using System;
using System.Globalization;
using System.Threading.Tasks;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface ISportEvent
{
    public URN Id { get; }

    [Obsolete("Do not use this field, it will be removed in future.")]
    public URN RefId { get; }

    public Task<string> GetNameAsync(CultureInfo culture);

    public Task<URN> GetSportIdAsync();

    public Task<ISport> GetSportAsync();

    public Task<DateTime?> GetScheduledTimeAsync();

    public Task<DateTime?> GetScheduledEndTimeAsync();
}