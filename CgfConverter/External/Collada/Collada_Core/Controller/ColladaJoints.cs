using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaJoints
{
    [XmlElement(ElementName = "input")]
    public ColladaInputUnshared[] Input;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

