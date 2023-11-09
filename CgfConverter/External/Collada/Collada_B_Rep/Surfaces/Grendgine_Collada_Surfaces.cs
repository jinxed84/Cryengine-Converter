using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Surfaces
    {
        [XmlElement(ElementName = "surface")]
        public Grendgine_Collada_Surface[] Surface;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;
    }
}

