using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader
{
    public static class TagInstanceFactory
    {
        public static TagInstance Create(Template tagDef, long addressStart, long offset)
        {

            switch ((tagDef as TagLayouts.C).T)
            {
                case TagElemntType.Comment:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntType.Explanation:
                    return new Explanation(tagDef, addressStart, offset);
                case TagElemntType.CustomLikeGrouping:
                    return new CustomLikeGrouping(tagDef, addressStart, offset);
                case TagElemntType.ArrayFixLen:
                    return new ArrayFixLen(tagDef, addressStart, offset);
                case TagElemntType.GenericBlock:
                    return new GenericBlock(tagDef, addressStart, offset);
                case TagElemntType.TagStructData:
                    if (tagDef.E != null && tagDef.E.ContainsKey("hash") && tagDef.E["hash"].ToString() == "E423D497BA42B08FA925E0B06C3C363A")
                        return new RenderGeometryTag(tagDef, addressStart, offset);
                    return new TagStructData(tagDef, addressStart, offset);
                case TagElemntType.TagData:
                    return new TagData(tagDef, addressStart, offset);
                case TagElemntType.EnumGroup:
                    return new EnumGroup(tagDef, addressStart, offset);
                case TagElemntType.FourByte:
                    return new FourByte(tagDef, addressStart, offset);
                case TagElemntType.TwoByte:
                    return new TwoByte(tagDef, addressStart, offset);
                case TagElemntType.UTwoByte:
                    return new UTwoByte(tagDef, addressStart, offset);
                case TagElemntType.Byte:
                    return new Byte(tagDef, addressStart, offset);
                case TagElemntType.Float:
                    return new Float(tagDef, addressStart, offset);
                case TagElemntType.TagRef:
                    return new TagRef(tagDef, addressStart, offset);
                case TagElemntType.Pointer:
                    return new Pointer(tagDef, addressStart, offset);
                case TagElemntType.Tagblock:
                    return new Tagblock(tagDef, addressStart, offset);
                case TagElemntType.ResourceHandle:
                    return new ResourceHandle(tagDef, addressStart, offset);
                case TagElemntType.TagStructBlock:
                    return new TagStructBlock(tagDef, addressStart, offset);
                case TagElemntType.String:
                    return new String(tagDef, addressStart, offset);
                case TagElemntType.StringTag:
                    return new StringTag(tagDef, addressStart, offset);
                case TagElemntType.Flags:
                    return new Flags(tagDef, addressStart, offset);
                case TagElemntType.FlagGroup:
                    return new FlagGroup(tagDef, addressStart, offset);
                case TagElemntType.Mmr3Hash:
                    return new Mmr3Hash(tagDef, addressStart, offset);
                case TagElemntType.RgbPixel32:
                    return new RgbPixel32(tagDef, addressStart, offset);
                case TagElemntType.ArgbPixel32:
                    return new ArgbPixel32(tagDef, addressStart, offset);
                case TagElemntType.RGB:
                    return new RGB(tagDef, addressStart, offset);
                case TagElemntType.ARGB:
                    return new ARGB(tagDef, addressStart, offset);
                case TagElemntType.Bounds2Byte:
                    return new Bounds2Byte(tagDef, addressStart, offset);
                case TagElemntType.BoundsFloat:
                    return new BoundsFloat(tagDef, addressStart, offset);
                case TagElemntType.Point2D2Byte:
                    return new Point2D2Byte(tagDef, addressStart, offset);
                case TagElemntType.Point2DFloat:
                    return new Point2DFloat(tagDef, addressStart, offset);
                case TagElemntType.Point3D:
                    return new Point3D(tagDef, addressStart, offset);
                case TagElemntType.Quaternion:
                    return new Quaternion(tagDef, addressStart, offset);
                case TagElemntType.Plane2D:
                    return new Plane2D(tagDef, addressStart, offset);
                case TagElemntType.Plane3D:
                    return new Plane3D(tagDef, addressStart, offset);

                default:
                    return new TagInstance(tagDef, addressStart, offset);
            }
        }
    }

}
