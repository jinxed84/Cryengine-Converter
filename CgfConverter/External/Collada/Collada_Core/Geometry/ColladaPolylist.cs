using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaPolylist : ColladaGeometryCommonFields
{
    [XmlElement(ElementName = "vcount")]
    public ColladaIntArrayString VCount;
}
