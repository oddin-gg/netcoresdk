using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Oddin.OddsFeedSdk.API.Models;

[Serializable]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "void_reasons", IsNullable = false)]
public class MarketVoidReasonsModel
{
    private ResponseCode _responseCodeField;
    private bool _responseCodeFieldSpecified;
    private void_reason[] _voidReasonsField;

    /// <remarks />
    [XmlElement("void_reason")]
    public void_reason[] void_reasons
    {
        get => _voidReasonsField;
        set => _voidReasonsField = value;
    }

    /// <remarks />
    [XmlAttribute]
    public ResponseCode response_code
    {
        get => _responseCodeField;
        set => _responseCodeField = value;
    }

    /// <remarks />
    [XmlIgnore]
    public bool response_codeSpecified
    {
        get => _responseCodeFieldSpecified;
        set => _responseCodeFieldSpecified = value;
    }
}

/// <remarks />
[Serializable]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType]
public class void_reason
{
    private string _descriptionField;

    private int _idField;
    private string _nameField;

    private void_reason_param[] _paramsField;
    private string _templateField;

    /// <remarks />
    [XmlAttribute]
    public int id
    {
        get => _idField;
        set => _idField = value;
    }

    /// <remarks />
    [XmlAttribute]
    public string name
    {
        get => _nameField;
        set => _nameField = value;
    }

    /// <remarks />
    [XmlAttribute]
    public string description
    {
        get => _descriptionField;
        set => _descriptionField = value;
    }

    /// <remarks />
    [XmlAttribute]
    public string template
    {
        get => _templateField;
        set => _templateField = value;
    }

    /// <remarks />
    [XmlElement("param")]
    public void_reason_param[] void_reason_params
    {
        get => _paramsField;
        set => _paramsField = value;
    }
}

/// <remarks />
[Serializable]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType]
public class void_reason_param
{
    private string _nameField;

    /// <remarks />
    [XmlAttribute]
    public string name
    {
        get => _nameField;
        set => _nameField = value;
    }
}