using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader
{

    public enum TagElemntType {
        Comment,
        ArrayFixLen,
        GenericBlock,
        TagStructData,
        FUNCTION,
        EnumGroup,
        UFourByte,
        UTwoByte,
        SByte,
        FourByte,
        TwoByte,
        Byte,
        Float,
        TagRef,
        Pointer,
        Tagblock,
        ResourceHandle,
        TagStructBlock,
        String,
        StringTag,
        Flags,
        FlagGroup,
        Mmr3Hash,
        RGB,
        ARGB,
        BoundsFloat,
        Bounds2Byte,
        Point2DFloat,
        Point2D2Byte,
        Point3D,
        Quaternion,
        Plane3D,
        RootTagInstance,
        TagInstance,
        TagParentInstance
    }

    internal class TagCommon
    {
    }

}
