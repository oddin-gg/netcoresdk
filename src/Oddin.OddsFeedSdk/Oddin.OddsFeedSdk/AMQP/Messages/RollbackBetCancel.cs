//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

//
//This source code was auto-generated by MonoXSD
//
namespace Oddin.OddsFeedSdk.AMQP.Messages
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class rollback_bet_cancel : FeedMessageModel {

        private rollback_bet_cancelMarket[] marketField;

        private int productField;

        private string event_idField;

        private long timestampField;

        private long request_idField;

        private bool request_idFieldSpecified;

        private long start_timeField;

        private bool start_timeFieldSpecified;

        private long end_timeField;

        private bool end_timeFieldSpecified;

        public override long GeneratedAt => timestamp;

        public override int ProducerId => product;


        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("market")]
        public rollback_bet_cancelMarket[] market {
            get {
                return this.marketField;
            }
            set {
                this.marketField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int product {
            get {
                return this.productField;
            }
            set {
                this.productField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string event_id {
            get {
                return this.event_idField;
            }
            set {
                this.event_idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public long timestamp {
            get {
                return this.timestampField;
            }
            set {
                this.timestampField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public long request_id {
            get {
                return this.request_idField;
            }
            set {
                this.request_idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool request_idSpecified {
            get {
                return this.request_idFieldSpecified;
            }
            set {
                this.request_idFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public long start_time {
            get {
                return this.start_timeField;
            }
            set {
                this.start_timeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool start_timeSpecified {
            get {
                return this.start_timeFieldSpecified;
            }
            set {
                this.start_timeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public long end_time {
            get {
                return this.end_timeField;
            }
            set {
                this.end_timeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool end_timeSpecified {
            get {
                return this.end_timeFieldSpecified;
            }
            set {
                this.end_timeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class rollback_bet_cancelMarket {

        private int idField;

        private string specifiersField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string specifiers {
            get {
                return this.specifiersField;
            }
            set {
                this.specifiersField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
}
