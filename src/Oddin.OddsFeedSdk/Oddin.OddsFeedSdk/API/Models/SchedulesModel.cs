//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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
    [System.Xml.Serialization.XmlRootAttribute("schedule", IsNullable = false)]
    public partial class scheduleEndpoint {

        private sportEvent[] sport_eventField;

        private System.DateTime generated_atField;

        private bool generated_atFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("sport_event")]
        public sportEvent[] sport_event {
            get {
                return this.sport_eventField;
            }
            set {
                this.sport_eventField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime generated_at {
            get {
                return this.generated_atField;
            }
            set {
                this.generated_atField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool generated_atSpecified {
            get {
                return this.generated_atFieldSpecified;
            }
            set {
                this.generated_atFieldSpecified = value;
            }
        }
    }
}
