namespace Oddin.OddinSdk.SDK.API.Models
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute()]
    [System.Xml.Serialization.XmlRootAttribute("bookmaker_details", IsNullable = false)]
    internal class BookmakerDetailsModel
    {

        private ResponseCode response_codeField;

        private System.DateTime expire_atField;

        private int bookmaker_idField;

        private string virtual_hostField;

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
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime expire_at
        {
            get
            {
                return this.expire_atField;
            }
            set
            {
                this.expire_atField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int bookmaker_id
        {
            get
            {
                return this.bookmaker_idField;
            }
            set
            {
                this.bookmaker_idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string virtual_host
        {
            get
            {
                return this.virtual_hostField;
            }
            set
            {
                this.virtual_hostField = value;
            }
        }
    }
}
