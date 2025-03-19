namespace Oddin.OddsFeedSdk.API.Entities;

public class PlayerWithSport
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Fullname { get; set; }

    public string SportId { get; set; }

    internal PlayerWithSport(string id, string name, string fullname, string sportId)
    {
        Id = id;
        Name = name;
        Fullname = fullname;
        SportId = sportId;
    }
}