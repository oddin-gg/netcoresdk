namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

internal interface ITeam
{
#pragma warning disable IDE1006 // Naming Styles
    string refid { get; }
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
    bool virtualSpecified { get; }
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
    bool @virtual { get; }
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
    string country_code { get; }
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
    string underage { get; }
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
    string name { get; }
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
    string abbreviation { get; }
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
    string country { get; }
#pragma warning restore IDE1006 // Naming Styles
}
