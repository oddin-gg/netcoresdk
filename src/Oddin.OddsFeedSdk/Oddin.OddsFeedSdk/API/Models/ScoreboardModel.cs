using System.Xml.Serialization;

namespace Oddin.OddsFeedSdk.API.Models
{
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType()]
    [XmlRoot("scoreboard", IsNullable = false)]
    public class ScoreboardModel
    {
        [XmlAttribute] public int current_ct_team { get; set; }
        [XmlAttribute] public int home_won_rounds { get; set; }
        [XmlAttribute] public int away_won_rounds { get; set; }
        [XmlAttribute] public int current_round { get; set; }
        [XmlAttribute] public int home_kills { get; set; }
        [XmlAttribute] public int away_kills { get; set; }
        [XmlAttribute] public int home_destroyed_turrets { get; set; }
        [XmlAttribute] public int away_destroyed_turrets { get; set; }
        [XmlAttribute] public int home_gold { get; set; }
        [XmlAttribute] public int away_gold { get; set; }
        [XmlAttribute] public int home_destroyed_towers { get; set; }
        [XmlAttribute] public int away_destroyed_towers { get; set; }
        [XmlAttribute] public int home_goals { get; set; }
        [XmlAttribute] public int away_goals { get; set; }
        [XmlAttribute] public int time { get; set; }
        [XmlAttribute] public int game_time { get; set; }
        [XmlAttribute] public int current_def_team { get; set; }
        // VirtualBasketballScoreboard
        [XmlAttribute] public int home_points { get; set; }
        [XmlAttribute] public int away_points { get; set; }
        [XmlAttribute] public int remaining_game_time { get; set; }
    }
}