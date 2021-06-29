namespace Oddin.OddinSdk.SDK.AMQP.Messages
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "alive", IsNullable = false)]
    public class AliveMessage : FeedMessage
    {

        private int productField;

        private long timestampField;

        private int subscribedField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int product
        {
            get
            {
                return this.productField;
            }
            set
            {
                this.productField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public long timestamp
        {
            get
            {
                return this.timestampField;
            }
            set
            {
                this.timestampField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int subscribed
        {
            get
            {
                return this.subscribedField;
            }
            set
            {
                this.subscribedField = value;
            }
        }
    }
}
