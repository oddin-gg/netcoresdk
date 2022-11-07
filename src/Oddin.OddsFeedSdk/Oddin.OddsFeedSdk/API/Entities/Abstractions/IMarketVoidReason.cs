using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface IMarketVoidReason
{
    int Id { get; }

    string Name { get; }

    string Description { get; }

    string Template { get; }

    IEnumerable<string> Params { get; }
}