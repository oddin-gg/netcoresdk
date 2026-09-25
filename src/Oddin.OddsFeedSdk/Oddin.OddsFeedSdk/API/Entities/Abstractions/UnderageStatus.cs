namespace Oddin.OddsFeedSdk.API.Entities.Abstractions;

/// <summary>
/// Whether a competitor or player is flagged as underage. The feed encodes it
/// as -1 (unknown), 0 (no) and 1 (yes); anything else reads as <see cref="Unknown"/>.
/// </summary>
public enum UnderageStatus
{
    Unknown = -1,
    No = 0,
    Yes = 1
}
