using System.Collections.Generic;
using Oddin.OddsFeedSdk.AMQP.Enums;
using Oddin.OddsFeedSdk.Common;

namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public class SportFormat
{
    private SportFormat(string value) { Value = value; }

    public string Value { get; private set; }

    public static SportFormat Classic => new("classic");
    public static SportFormat Race => new("race");
    public static SportFormat Unknown => new("unknown");

    public override string ToString()
    {
        return Value;
    }

    public bool IsRace() => Value == SportFormat.Race.Value;
    public bool IsClassic() => Value == SportFormat.Classic.Value;

    public bool IsUnknown() => Value == SportFormat.Unknown.Value;
}


public interface IMatch : ISportEvent
{
    URN SportId { get; }

    LiveOddsAvailability? LiveOddsAvailability { get; }

    IMatchStatus Status { get; }

    IEnumerable<ITeamCompetitor> Competitors { get; }

    ITeamCompetitor HomeCompetitor { get; }

    ITeamCompetitor AwayCompetitor { get; }

    ITournament Tournament { get; }

    IFixture Fixture { get; }

    SportFormat SportFormat { get; }

    IDictionary<string, string> ExtraInfo { get; }
}
