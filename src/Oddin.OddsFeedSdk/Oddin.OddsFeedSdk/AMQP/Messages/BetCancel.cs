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
using Oddin.OddsFeedSdk.AMQP.Messages;


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(IsNullable = false)]
public partial class bet_cancel : FeedMessageModel
{
    public override long GeneratedAt => timestampField;
    public override int ProducerId => product;

    private bet_cancel_market[] marketField;

    private int productField;

    private string event_idField;

    private long timestampField;

    private long request_idField;

    private bool request_idFieldSpecified;

    private long start_timeField;

    private bool start_timeFieldSpecified;

    private long end_timeField;

    private bool end_timeFieldSpecified;

    private string superceded_byField;

    private string event_refidField;

    [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "event_ref_id")]
    public string event_refid
    {
        get
        {
            return this.event_refidField;
        }
        set
        {
            this.event_refidField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("market")]
    public bet_cancel_market[] market
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
    public string event_id
    {
        get
        {
            return this.event_idField;
        }
        set
        {
            this.event_idField = value;
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
    public long request_id
    {
        get
        {
            return this.request_idField;
        }
        set
        {
            this.request_idField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool request_idSpecified
    {
        get
        {
            return this.request_idFieldSpecified;
        }
        set
        {
            this.request_idFieldSpecified = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public long start_time
    {
        get
        {
            return this.start_timeField;
        }
        set
        {
            this.start_timeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool start_timeSpecified
    {
        get
        {
            return this.start_timeFieldSpecified;
        }
        set
        {
            this.start_timeFieldSpecified = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public long end_time
    {
        get
        {
            return this.end_timeField;
        }
        set
        {
            this.end_timeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool end_timeSpecified
    {
        get
        {
            return this.end_timeFieldSpecified;
        }
        set
        {
            this.end_timeFieldSpecified = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string superceded_by
    {
        get
        {
            return this.superceded_byField;
        }
        set
        {
            this.superceded_byField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class bet_cancel_market
{
    private int idField;

    private string specifiersField;

    private string extended_specifiersField;

    private string groupsField;

    private int void_reasonField;

    private bool void_reasonFieldSpecified;

    private int void_reason_idField;

    private bool void_reason_idFieldSpecified;

    private string void_reason_paramsField;

    private bool void_reason_paramsFieldSpecified;

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
    public string specifiers
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
    public string extended_specifiers
    {
        get
        {
            return this.extended_specifiersField;
        }
        set
        {
            this.extended_specifiersField = value;
        }
    }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string groups
    {
        get
        {
            return this.groupsField;
        }
        set
        {
            this.groupsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public int void_reason
    {
        get
        {
            return this.void_reasonField;
        }
        set
        {
            this.void_reasonField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool void_reasonSpecified
    {
        get
        {
            return this.void_reasonFieldSpecified;
        }
        set
        {
            this.void_reasonFieldSpecified = value;
        }
    }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public int void_reason_id
    {
        get
        {
            return this.void_reason_idField;
        }
        set
        {
            this.void_reason_idField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool void_reason_idSpecified
    {
        get
        {
            return this.void_reason_idFieldSpecified;
        }
        set
        {
            this.void_reason_idFieldSpecified = value;
        }
    }

    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string void_reason_params
    {
        get
        {
            return this.void_reason_paramsField;
        }
        set
        {
            this.void_reason_paramsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool void_reason_paramsSpecified
    {
        get
        {
            return this.void_reason_paramsFieldSpecified;
        }
        set
        {
            this.void_reason_paramsFieldSpecified = value;
        }
    }
}