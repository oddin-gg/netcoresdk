//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

//
// This source code was auto-generated by xsd, Version=4.8.3928.0.
//
namespace Oddin.OddsFeedSdk.API.Models
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute("sport_tournaments", IsNullable = false)]
    public partial class TournamentsModel
    {
        [XmlArrayItemAttribute(IsNullable = false)]
        public List<tournament> tournaments { get; set; }

        [XmlElement]
        public sport sport { get; set; }

        [XmlAttribute]
        public DateTime generated_at { get; set; }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
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

    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(teamCompetitor))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(teamExtended))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class team
    {

        private string idField;

        private string nameField;

        private string abbreviationField;

        private string underageField;

        private string countryField;

        private string country_codeField;

        private bool virtualField;

        private bool virtualFieldSpecified;

        private string refidField;

        [System.Xml.Serialization.XmlAttribute(AttributeName = "ref_id")]
        public string refid
        {
            get
            {
                return this.refidField;
            }
            set
            {
                this.refidField = value;
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
        [System.Xml.Serialization.XmlAttribute()]
        public string underage
        {
            get
            {
                return this.underageField;
            }
            set
            {
                this.underageField = value;
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
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(tournamentExtended))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class tournament
    {

        private tournamentLength tournament_lengthField;

        private sport sportField;

        private string idField;

        private string nameField;

        private int riskTierField;

        private System.DateTime scheduledField;

        private bool scheduledFieldSpecified;

        private System.DateTime scheduled_endField;

        private bool scheduled_endFieldSpecified;

        private string refidField;

        [XmlAttribute]
        public string abbreviation { get; set; }

        [System.Xml.Serialization.XmlAttribute(AttributeName = "ref_id")]
        public string refid
        {
            get
            {
                return this.refidField;
            }
            set
            {
                this.refidField = value;
            }
        }

        [System.Xml.Serialization.XmlAttribute(AttributeName = "risk_tier")]
        public int riskTier
        {
            get
            {
                return this.riskTierField;
            }
            set
            {
                this.riskTierField = value;
            }
        }

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
    public partial class teamExtended : team
    {
        private sport[] sportField;

        private string icon_pathField;

        [System.Xml.Serialization.XmlElementAttribute("sport")]
        public sport[] sport
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
}