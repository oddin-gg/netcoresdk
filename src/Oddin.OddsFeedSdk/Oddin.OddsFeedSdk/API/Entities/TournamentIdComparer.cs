using System.Collections.Generic;
using Oddin.OddsFeedSdk.API.Entities.Abstractions;

namespace Oddin.OddsFeedSdk.API.Entities;

internal class TournamentIdComparer : IEqualityComparer<ITournament>
{
    public bool Equals(ITournament one, ITournament two)
        => one.Id == two.Id;

    public int GetHashCode(ITournament item)
        => item.GetHashCode();
}