using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader
{
    public static class TagInstanceFactoryV2
    {
        public static TagInstance Create(Template tagDef, long addressStart, int offset)
        {
            TagElemntTypeV2? tagElemntTypeV2 = (tagDef as TagLayoutsV2.C).T;
            if (tagElemntTypeV2 == null)
                return null;
            switch (tagElemntTypeV2)
            {
                case TagElemntTypeV2.RootTagInstance:
                    return new RootTagInstance(tagDef, addressStart, offset); 
                case TagElemntTypeV2.String:
                    return new String(tagDef, addressStart, offset);
                case TagElemntTypeV2.LongString:
                    return new String(tagDef, addressStart, offset);
                case TagElemntTypeV2.Mmr3Hash:
                    return new Mmr3Hash(tagDef, addressStart, offset);
                case TagElemntTypeV2.NotFound03:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntTypeV2.CharIntiger:
                    return new Byte(tagDef, addressStart, offset);
                case TagElemntTypeV2.Short:
                    return new TwoByte(tagDef, addressStart, offset);
                case TagElemntTypeV2.Long:
                    return new FourByte(tagDef, addressStart, offset);
                case TagElemntTypeV2.Int64:
                    return new Pointer(tagDef, addressStart, offset);
                case TagElemntTypeV2.Angle:
                    return new Float(tagDef, addressStart, offset);
                case TagElemntTypeV2.StringTag:
                    return new StringTag(tagDef, addressStart, offset);
                case TagElemntTypeV2.CharEnum:
                case TagElemntTypeV2.ShortEnum:
                case TagElemntTypeV2.LongEnum:
                    return new EnumGroup(tagDef, addressStart, offset);
                case TagElemntTypeV2.LongFlags:
                case TagElemntTypeV2.WordFlags:
                case TagElemntTypeV2.ByteFlags:
                    return new FlagGroup(tagDef, addressStart, offset);
                case TagElemntTypeV2.ShortPoint2D:
                    return new Point2D2Byte(tagDef, addressStart, offset);
                case TagElemntTypeV2.ShortRectangle2D:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntTypeV2.RgbPixel32:
                    return new RgbPixel32(tagDef, addressStart, offset);
                case TagElemntTypeV2.ArgbPixel32:
                    return new ArgbPixel32(tagDef, addressStart, offset);
                case TagElemntTypeV2.Real:
                case TagElemntTypeV2.Fraction:
                    return new Float(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealPoint2D:
                    return new Point2DFloat(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealPoint3D:
                    return new Point3D(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealVector2D:
                    return new Point2DFloat(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealVector3D:
                    return new Point3D(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealQuaternion:
                    return new Quaternion(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealEulerAngles2D:
                    return new Point2DFloat(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealEulerAngles3D:
                    return new Point3D(tagDef, addressStart, offset);
                case TagElemntTypeV2.Plane2D:
                    return new Plane2D(tagDef, addressStart, offset);
                case TagElemntTypeV2.Plane3D:
                    return new Plane3D(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealRgbColor:
                    return new RGB(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealARgbColor:
                    return new ARGB(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealHsvColor:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealAhsvColor:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntTypeV2.ShortBounds:
                    return new Bounds2Byte(tagDef, addressStart, offset);
                case TagElemntTypeV2.AngleBounds:
                    return new BoundsFloat(tagDef, addressStart, offset);
                case TagElemntTypeV2.RealBounds:
                    return new BoundsFloat(tagDef, addressStart, offset);
                case TagElemntTypeV2.FractionBounds:
                    return new BoundsFloat(tagDef, addressStart, offset);
                case TagElemntTypeV2.Unmapped27:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntTypeV2.Unmapped28:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntTypeV2.DwordBlockFlags:
                    return new FourByte(tagDef, addressStart, offset);
                case TagElemntTypeV2.WordBlockFlags:
                    return new TwoByte(tagDef, addressStart, offset);
                case TagElemntTypeV2.ByteBlockFlags:
                    return new Byte(tagDef, addressStart, offset);
                case TagElemntTypeV2.CharBlockIndex:
                    return new Byte(tagDef, addressStart, offset);
                case TagElemntTypeV2.CustomCharBlockIndex:
                    return new Byte(tagDef, addressStart, offset);
                case TagElemntTypeV2.ShortBlockIndex:
                    return new TwoByte(tagDef, addressStart, offset);
                case TagElemntTypeV2.CustomShortBlockIndex:
                    return new TwoByte(tagDef, addressStart, offset);
                case TagElemntTypeV2.LongBlockIndex:
                    return new FourByte(tagDef, addressStart, offset);
                case TagElemntTypeV2.CustomLongBlockIndex:
                    return new FourByte(tagDef, addressStart, offset);
                case TagElemntTypeV2.NotFound32:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntTypeV2.NotFound33:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntTypeV2.Pad:
                    return new GenericBlock(tagDef, addressStart, offset);
                case TagElemntTypeV2.Skip:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntTypeV2.Explanation:
                    return new Explanation(tagDef, addressStart, offset);
                case TagElemntTypeV2.Custom:
                    return new CustomLikeGrouping(tagDef, addressStart, offset);
                case TagElemntTypeV2.Struct:
                    if (tagDef.E != null && tagDef.E.ContainsKey("hash") && tagDef.E["hash"].ToString() == "E423D497BA42B08FA925E0B06C3C363A")
                        return new RenderGeometryTag(tagDef, addressStart, offset);
                    return new StructTagInstance(tagDef, addressStart, offset);
                case TagElemntTypeV2.Array:
                    return new ArrayFixLen(tagDef, addressStart, offset);
                case TagElemntTypeV2.Unmapped3A:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntTypeV2.EndStruct:
                    return new Comment(tagDef, addressStart, offset);
                case TagElemntTypeV2.Byte:
                    return new Byte(tagDef, addressStart, offset);
                case TagElemntTypeV2.Word:
                    return new TwoByte(tagDef, addressStart, offset);
                case TagElemntTypeV2.Dword:
                    return new FourByte(tagDef, addressStart, offset);
                case TagElemntTypeV2.Qword:
                    return new Pointer(tagDef, addressStart, offset);
                case TagElemntTypeV2.Block:
                    return new Tagblock(tagDef, addressStart, offset);
                case TagElemntTypeV2.TagReference:
                    return new TagRef(tagDef, addressStart, offset);
                case TagElemntTypeV2.Data: 
                    return new TagData(tagDef, addressStart, offset);
                case TagElemntTypeV2.ResourceHandle:
                    return new ResourceHandle(tagDef, addressStart, offset);
                case TagElemntTypeV2.DataPath:
                    return new String(tagDef, addressStart, offset);
                case TagElemntTypeV2.Unmapped45:
                    return new GenericBlock(tagDef, addressStart, offset);
                case TagElemntTypeV2.NotFound69:
                    return new GenericBlock(tagDef, addressStart, offset);
                case null:
                    return null;
                default:
                    return new TagInstance(tagDef, addressStart, offset);
            }

            return new TagInstance(tagDef, addressStart, offset);
        }
    }
}
