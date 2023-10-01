using System;

namespace Oddin.OddsFeedSdk.Common;

public class URN
{
    public const string TypeMatch = "match";
    public const string TypeTournament = "tournament";

    public URN(string prefix, string type, long id)
    {
        Prefix = prefix;
        Type = type;
        Id = id;
    }

    public URN(string urn)
    {
        if (urn is null)
            throw new ArgumentNullException(nameof(urn));

        if (TryParseUrn(urn, out var prefix, out var type, out var id) == false)
            throw new ArgumentException(
                $"Given argument {nameof(urn)} of type {typeof(string).Name} is not a valid {typeof(URN).Name}!");

        Prefix = prefix;
        Type = type;
        Id = id;
    }

    public string Prefix { get; }

    public string Type { get; }

    public long Id { get; }

    public override string ToString()
        => $"{Prefix}:{Type}:{Id}";

    private bool TryParseUrn(string urnString, out string prefix, out string type, out long id)
    {
        prefix = string.Empty;
        type = string.Empty;
        id = 0;

        var urnSplit = urnString.Split(":", StringSplitOptions.RemoveEmptyEntries);
        if (urnSplit.Length != 3)
            return false;

        if (long.TryParse(urnSplit[2], out id) == false
            || id <= 0)
            return false;

        prefix = urnSplit[0];
        type = urnSplit[1];
        return true;
    }

    public override bool Equals(object obj)
        => obj is URN uRN &&
           Prefix == uRN.Prefix &&
           Type == uRN.Type &&
           Id == uRN.Id;

    public override int GetHashCode()
        => HashCode.Combine(Prefix, Type, Id);

    public static implicit operator string(URN urn) => urn.ToString();

    public static bool operator ==(URN firstUrn, URN secondUrn)
    {
        if (firstUrn is null)
        {
            if (secondUrn is null)
                return true;

            return false;
        }

        return firstUrn.Equals(secondUrn);
    }

    public static bool operator !=(URN firstUrn, URN secondUrn)
        => firstUrn == secondUrn == false;
}