namespace Oddin.OddsFeedSdk.API.Models
{
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "void_reasons", IsNullable = false)]
    public class MarketVoidReasonsModel
    {
        private ResponseCode _responseCodeField;
        private bool _responseCodeFieldSpecified;
        private void_reason[] _voidReasonsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("void_reason")]
        public void_reason[] void_reasons
        {
            get => _voidReasonsField;
            set => _voidReasonsField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ResponseCode response_code
        {
            get => _responseCodeField;
            set => _responseCodeField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool response_codeSpecified
        {
            get => _responseCodeFieldSpecified;
            set => _responseCodeFieldSpecified = value;
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public class void_reason
    {
        private string _descriptionField;

        private int _idField;
        private string _nameField;

        private void_reason_param[] _paramsField;
        private string _templateField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int id
        {
            get => _idField;
            set => _idField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get => _nameField;
            set => _nameField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string description
        {
            get => _descriptionField;
            set => _descriptionField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string template
        {
            get => _templateField;
            set => _templateField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("param")]
        public void_reason_param[] void_reason_params
        {
            get => _paramsField;
            set => _paramsField = value;
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public class void_reason_param
    {
        private string _nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get => _nameField;
            set => _nameField = value;
        }
    }
}