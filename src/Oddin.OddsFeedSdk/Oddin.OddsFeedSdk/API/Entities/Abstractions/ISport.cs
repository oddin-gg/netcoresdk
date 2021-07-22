using System.Collections.Generic;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions
{
    public interface ISport : ISportSummary
    {
        IEnumerable<ISportEvent> Tournaments { get; }
    }
}
