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
// This source code was auto-generated by xsd, Version=4.8.3928.0.
//

namespace Oddin.OddsFeedSdk.API.Models
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "market_descriptions", IsNullable = false)]
    public class MarketDescriptionsModel
    {

        private market_description[] marketField;

        private ResponseCode response_codeField;

        private bool response_codeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("market")]
        public market_description[] market
        {
            get
            {
                return this.marketField;
            }
            set
            {
                this.marketField = value;
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

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public partial class market_description
    {

        private outcome_descriptionOutcome[] outcomesField;

        private specifier_descriptionSpecifier[] specifiersField;

        private int idField;

        private string nameField;

        private string variantField;

        private int refidField;

        private string includes_outcomes_of_typeField;

        private string outcome_typeField;

        private string groupsField;

        [System.Xml.Serialization.XmlAttribute(AttributeName = "ref_id")]
        public int refid
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
        [System.Xml.Serialization.XmlArrayItemAttribute("outcome", IsNullable = false)]
        public outcome_descriptionOutcome[] outcomes
        {
            get
            {
                return this.outcomesField;
            }
            set
            {
                this.outcomesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("specifier", IsNullable = false)]
        public specifier_descriptionSpecifier[] specifiers
        {
            get
            {
                return this.specifiersField;
            }
            set
            {
                this.specifiersField = value;
            }
        }

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
        public string variant
        {
            get
            {
                return this.variantField;
            }
            set
            {
                this.variantField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string includes_outcomes_of_type {
            get {
                return this.includes_outcomes_of_typeField;
            }
            set {
                this.includes_outcomes_of_typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string outcome_type {
            get {
                return this.outcome_typeField;
            }
            set {
                this.outcome_typeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "groups")]
        public string groups {
            get {
                return this.groupsField;
            }
            set {
                this.groupsField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class outcome_descriptionOutcome
    {

        private string idField;

        private string nameField;

        private string descriptionField;

        private int refidField;

        [System.Xml.Serialization.XmlAttribute(AttributeName = "ref_id")]
        public int refid
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
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class specifier_descriptionSpecifier
    {

        private string nameField;

        private string typeField;

        private string descriptionField;

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
    }
}
