namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

public interface INamedValue
{
    long Id { get; }

    string Description { get; }
}