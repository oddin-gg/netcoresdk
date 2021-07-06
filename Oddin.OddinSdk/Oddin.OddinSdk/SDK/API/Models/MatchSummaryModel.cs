namespace Oddin.OddinSdk.SDK.API.Models
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute()]
    [System.Xml.Serialization.XmlRootAttribute("match_summary", IsNullable = false)]
    internal class MatchSummaryModel
    {

        private sportEvent sport_eventField;

        private sportEventStatus sport_event_statusField;

        private System.DateTime generated_atField;

        private bool generated_atFieldSpecified;

        /// <remarks/>
        public sportEvent sport_event
        {
            get
            {
                return this.sport_eventField;
            }
            set
            {
                this.sport_eventField = value;
            }
        }

        /// <remarks/>
        public sportEventStatus sport_event_status
        {
            get
            {
                return this.sport_event_statusField;
            }
            set
            {
                this.sport_event_statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime generated_at
        {
            get
            {
                return this.generated_atField;
            }
            set
            {
                this.generated_atField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool generated_atSpecified
        {
            get
            {
                return this.generated_atFieldSpecified;
            }
            set
            {
                this.generated_atFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class sportEvent
    {

        private tournament tournamentField;

        private teamCompetitor[] competitorsField;

        private string idField;

        private string nameField;

        private string typeField;

        private System.DateTime scheduledField;

        private bool scheduledFieldSpecified;

        private bool start_time_tbdField;

        private bool start_time_tbdFieldSpecified;

        private System.DateTime scheduled_endField;

        private bool scheduled_endFieldSpecified;

        private string liveoddsField;

        private string statusField;

        /// <remarks/>
        public tournament tournament
        {
            get
            {
                return this.tournamentField;
            }
            set
            {
                this.tournamentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("competitor", IsNullable = false)]
        public teamCompetitor[] competitors
        {
            get
            {
                return this.competitorsField;
            }
            set
            {
                this.competitorsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime scheduled
        {
            get
            {
                return this.scheduledField;
            }
            set
            {
                this.scheduledField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool scheduledSpecified
        {
            get
            {
                return this.scheduledFieldSpecified;
            }
            set
            {
                this.scheduledFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool start_time_tbd
        {
            get
            {
                return this.start_time_tbdField;
            }
            set
            {
                this.start_time_tbdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool start_time_tbdSpecified
        {
            get
            {
                return this.start_time_tbdFieldSpecified;
            }
            set
            {
                this.start_time_tbdFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime scheduled_end
        {
            get
            {
                return this.scheduled_endField;
            }
            set
            {
                this.scheduled_endField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool scheduled_endSpecified
        {
            get
            {
                return this.scheduled_endFieldSpecified;
            }
            set
            {
                this.scheduled_endFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string liveodds
        {
            get
            {
                return this.liveoddsField;
            }
            set
            {
                this.liveoddsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(tournamentExtended))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class tournament
    {

        private tournamentLength tournament_lengthField;

        private sport sportField;

        private string idField;

        private string nameField;

        private System.DateTime scheduledField;

        private bool scheduledFieldSpecified;

        private System.DateTime scheduled_endField;

        private bool scheduled_endFieldSpecified;

        /// <remarks/>
        public tournamentLength tournament_length
        {
            get
            {
                return this.tournament_lengthField;
            }
            set
            {
                this.tournament_lengthField = value;
            }
        }

        /// <remarks/>
        public sport sport
        {
            get
            {
                return this.sportField;
            }
            set
            {
                this.sportField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime scheduled
        {
            get
            {
                return this.scheduledField;
            }
            set
            {
                this.scheduledField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool scheduledSpecified
        {
            get
            {
                return this.scheduledFieldSpecified;
            }
            set
            {
                this.scheduledFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime scheduled_end
        {
            get
            {
                return this.scheduled_endField;
            }
            set
            {
                this.scheduled_endField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool scheduled_endSpecified
        {
            get
            {
                return this.scheduled_endFieldSpecified;
            }
            set
            {
                this.scheduled_endFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class tournamentLength
    {

        private System.DateTime start_dateField;

        private bool start_dateFieldSpecified;

        private System.DateTime end_dateField;

        private bool end_dateFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime start_date
        {
            get
            {
                return this.start_dateField;
            }
            set
            {
                this.start_dateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool start_dateSpecified
        {
            get
            {
                return this.start_dateFieldSpecified;
            }
            set
            {
                this.start_dateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime end_date
        {
            get
            {
                return this.end_dateField;
            }
            set
            {
                this.end_dateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool end_dateSpecified
        {
            get
            {
                return this.end_dateFieldSpecified;
            }
            set
            {
                this.end_dateFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(periodScore))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class periodScoreBase
    {

        private string typeField;

        private int numberField;

        private bool numberFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool numberSpecified
        {
            get
            {
                return this.numberFieldSpecified;
            }
            set
            {
                this.numberFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class periodScore : periodScoreBase
    {

        private double home_scoreField;

        private double away_scoreField;

        private int match_status_codeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double home_score
        {
            get
            {
                return this.home_scoreField;
            }
            set
            {
                this.home_scoreField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double away_score
        {
            get
            {
                return this.away_scoreField;
            }
            set
            {
                this.away_scoreField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int match_status_code
        {
            get
            {
                return this.match_status_codeField;
            }
            set
            {
                this.match_status_codeField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(sportEventStatus))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class sportEventStatusBase
    {

        private periodScore[] period_scoresField;

        private string statusField;

        private string match_statusField;

        private string winner_idField;

        private string winning_reasonField;

        private bool decided_by_fedField;

        private bool decided_by_fedFieldSpecified;

        private int periodField;

        private bool periodFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("period_score", IsNullable = false)]
        public periodScore[] period_scores
        {
            get
            {
                return this.period_scoresField;
            }
            set
            {
                this.period_scoresField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string match_status
        {
            get
            {
                return this.match_statusField;
            }
            set
            {
                this.match_statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string winner_id
        {
            get
            {
                return this.winner_idField;
            }
            set
            {
                this.winner_idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string winning_reason
        {
            get
            {
                return this.winning_reasonField;
            }
            set
            {
                this.winning_reasonField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool decided_by_fed
        {
            get
            {
                return this.decided_by_fedField;
            }
            set
            {
                this.decided_by_fedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool decided_by_fedSpecified
        {
            get
            {
                return this.decided_by_fedFieldSpecified;
            }
            set
            {
                this.decided_by_fedFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int period
        {
            get
            {
                return this.periodField;
            }
            set
            {
                this.periodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool periodSpecified
        {
            get
            {
                return this.periodFieldSpecified;
            }
            set
            {
                this.periodFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class sportEventStatus : sportEventStatusBase
    {

        private double home_scoreField;

        private bool home_scoreFieldSpecified;

        private double away_scoreField;

        private bool away_scoreFieldSpecified;

        private double aggregate_home_scoreField;

        private bool aggregate_home_scoreFieldSpecified;

        private double aggregate_away_scoreField;

        private bool aggregate_away_scoreFieldSpecified;

        private string aggregate_winner_idField;

        private int status_codeField;

        private bool status_codeFieldSpecified;

        private int match_status_codeField;

        private bool match_status_codeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double home_score
        {
            get
            {
                return this.home_scoreField;
            }
            set
            {
                this.home_scoreField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool home_scoreSpecified
        {
            get
            {
                return this.home_scoreFieldSpecified;
            }
            set
            {
                this.home_scoreFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double away_score
        {
            get
            {
                return this.away_scoreField;
            }
            set
            {
                this.away_scoreField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool away_scoreSpecified
        {
            get
            {
                return this.away_scoreFieldSpecified;
            }
            set
            {
                this.away_scoreFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double aggregate_home_score
        {
            get
            {
                return this.aggregate_home_scoreField;
            }
            set
            {
                this.aggregate_home_scoreField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool aggregate_home_scoreSpecified
        {
            get
            {
                return this.aggregate_home_scoreFieldSpecified;
            }
            set
            {
                this.aggregate_home_scoreFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double aggregate_away_score
        {
            get
            {
                return this.aggregate_away_scoreField;
            }
            set
            {
                this.aggregate_away_scoreField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool aggregate_away_scoreSpecified
        {
            get
            {
                return this.aggregate_away_scoreFieldSpecified;
            }
            set
            {
                this.aggregate_away_scoreFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string aggregate_winner_id
        {
            get
            {
                return this.aggregate_winner_idField;
            }
            set
            {
                this.aggregate_winner_idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int status_code
        {
            get
            {
                return this.status_codeField;
            }
            set
            {
                this.status_codeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool status_codeSpecified
        {
            get
            {
                return this.status_codeFieldSpecified;
            }
            set
            {
                this.status_codeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int match_status_code
        {
            get
            {
                return this.match_status_codeField;
            }
            set
            {
                this.match_status_codeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool match_status_codeSpecified
        {
            get
            {
                return this.match_status_codeFieldSpecified;
            }
            set
            {
                this.match_status_codeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(teamCompetitor))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(teamExtended))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class team
    {

        private string idField;

        private string nameField;

        private string abbreviationField;

        private string countryField;

        private string country_codeField;

        private bool virtualField;

        private bool virtualFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string abbreviation
        {
            get
            {
                return this.abbreviationField;
            }
            set
            {
                this.abbreviationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string country
        {
            get
            {
                return this.countryField;
            }
            set
            {
                this.countryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string country_code
        {
            get
            {
                return this.country_codeField;
            }
            set
            {
                this.country_codeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool @virtual
        {
            get
            {
                return this.virtualField;
            }
            set
            {
                this.virtualField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool virtualSpecified
        {
            get
            {
                return this.virtualFieldSpecified;
            }
            set
            {
                this.virtualFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class teamCompetitor : team
    {

        private string qualifierField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string qualifier
        {
            get
            {
                return this.qualifierField;
            }
            set
            {
                this.qualifierField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class teamExtended : team
    {

        private sport sportField;

        private string icon_pathField;

        /// <remarks/>
        public sport sport
        {
            get
            {
                return this.sportField;
            }
            set
            {
                this.sportField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string icon_path
        {
            get
            {
                return this.icon_pathField;
            }
            set
            {
                this.icon_pathField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class sport
    {

        private string idField;

        private string nameField;

        private string abbreviationField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string abbreviation
        {
            get
            {
                return this.abbreviationField;
            }
            set
            {
                this.abbreviationField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.oddin.gg/v1")]
    public partial class tournamentExtended : tournament
    {

        private team[] competitorsField;

        private string icon_pathField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("competitor", IsNullable = false)]
        public team[] competitors
        {
            get
            {
                return this.competitorsField;
            }
            set
            {
                this.competitorsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string icon_path
        {
            get
            {
                return this.icon_pathField;
            }
            set
            {
                this.icon_pathField = value;
            }
        }
    }

}
