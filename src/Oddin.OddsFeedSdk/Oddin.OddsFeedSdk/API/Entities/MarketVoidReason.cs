using System.Collections.Generic;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class MarketVoidReason : IMarketVoidReason
{
    public MarketVoidReason(int id, string name, string description, string template, IEnumerable<string> @params)
    {
        Id = id;
        Name = name;
        Description = description;
        Template = template;
        Params = @params;
    }

    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Template { get; }
    public IEnumerable<string> Params { get; }
}