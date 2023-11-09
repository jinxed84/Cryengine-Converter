using System;
using System.Xml.Serialization;

namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Input_Semantic
    {
        BINORMAL,
        COLOR,
        CONTINUITY,
        IMAGE,
        INPUT,
        IN_TANGENT,
        INTERPOLATION,
        INV_BIND_MATRIX,
        JOINT,
        LINEAR_STEPS,
        MORPH_TARGET,
        MORPH_WEIGHT,
        NORMAL,
        OUTPUT,
        OUT_TANGENT,
        POSITION,
        TANGENT,
        TEXBINORMAL,
        TEXCOORD,
        TEXTANGENT,
        UV,
        VERTEX,
        WEIGHT,
    }
}

