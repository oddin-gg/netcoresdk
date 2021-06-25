namespace Oddin.OddinSdk.SDK.API.Models
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("producers", IsNullable = false)]
    public class ProducersModel
    {
        private ProducerModel[] producerField;

        private ResponseCode response_codeField;

        private bool response_codeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("producer")]
        public ProducerModel[] producer
        {
            get
            {
                return this.producerField;
            }
            set
            {
                this.producerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ResponseCode response_code
        {
            get
            {
                return this.response_codeField;
            }
            set
            {
                this.response_codeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool response_codeSpecified
        {
            get
            {
                return this.response_codeFieldSpecified;
            }
            set
            {
                this.response_codeFieldSpecified = value;
            }
        }
    }

    public class ProducerModel
    {
        private int idField;

        private string nameField;

        private string descriptionField;

        private string api_urlField;

        private bool activeField;

        private string scopeField;

        private int stateful_recovery_window_in_minutesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int id
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
        public string description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string api_url
        {
            get
            {
                return this.api_urlField;
            }
            set
            {
                this.api_urlField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool active
        {
            get
            {
                return this.activeField;
            }
            set
            {
                this.activeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string scope
        {
            get
            {
                return this.scopeField;
            }
            set
            {
                this.scopeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int stateful_recovery_window_in_minutes
        {
            get
            {
                return this.stateful_recovery_window_in_minutesField;
            }
            set
            {
                this.stateful_recovery_window_in_minutesField = value;
            }
        }
    }
}
