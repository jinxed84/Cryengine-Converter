using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaLibraryFormulas
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;


    [XmlElement(ElementName = "formula")]
    public ColladaFormula[] Formula;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

