namespace Oddin.OddsFeedSdk.API.Entities;

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