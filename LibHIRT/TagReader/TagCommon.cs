using LibHIRT.TagReader.Headers;

namespace LibHIRT.TagReader
{

    public delegate void OnInstanceEventHandler(object sender, EventArgs e);

    public interface ITagParseControl:IDisposable { 
        public CompoundTagInstance RootTagInst { get; }
        public Template TagTemplate { get;}

        public event EventHandler<ITagInstance> OnInstanceLoadEvent;

        TagFile TagFile { get; }
        public TagParseControlFiltter ParseControlFiltter { get ; set ; }
        public void readFile();
    }

    public interface Template {
        public Dictionary<string, object>? E { get; set; }
        /// <summary>
        /// Length of the tagblock
        /// </summary>
        public int S { get; set; } // S = size // length of tagblock

        public string N { get; set; } // N = name // our name for the block 

        public (string, string) xmlPath { get; set; }
        public string G { get; set; }

        public Dictionary<int, Template>? B { get; set; }
        public Dictionary<int, string> STR { get; set; }

    }

    public interface ITagInstance : IDisposable
    {
        static event EventHandler<ITagInstance> OnInstanceLoadEvent;
        public long GetTagSize { get; }

        public void ReadIn(BinaryReader f, TagHeader? header = null);
        public void WriteIn(Stream f, long offset = -1, TagHeader? header = null);
        public void ReadIn(TagHeader? header = null);

        public string ToJson();
        public TagInstance GetObjByPath(string path);

        public object AccessValue { get; set; }
        public string FieldName { get; set; }
        public bool NoAllowEdit { get; set; }

        public Template TagDef { get; }


    }

    public enum TagElemntType
    {
        Comment,
        Explanation,
        CustomLikeGrouping,
        ArrayFixLen,
        GenericBlock,
        TagStructData,
        TagData,
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
        Plane2D,
        Plane3D,
        RootTagInstance,
        TagInstance,
        TagParentInstance,
        RgbPixel32,
        ArgbPixel32
    }

    public enum TagElemntTypeV2
    {
        Undefined = -1,
        RootTagInstance = 0x100,
        String = 0x0, //  # size 32 string
        LongString = 0x1, //  # size 256 , # _field_long_string
        Mmr3Hash = 0x2, //  # size 4 , # _field_string_id
        NotFound03 = 0x3, //  # size 4 , # ## Not found in any tag type
        CharIntiger = 0x4, //  # size 1 , # _field_char_integer
        Short = 0x5, //  # size 2 , # _field_short_integer
        Long = 0x6, //  # size 4 , # _field_long_integer
        Int64 = 0x7, //  # size 8 , # _field_int64_integer
        Angle = 0x8, //  # size 4 , # _field_angle
        StringTag = 0x9, //  # size 4 , # _field_tag
        CharEnum = 0xA, //  # size 1 , # _field_char_enum
        ShortEnum = 0xB, //  # size 2 , # _field_short_enum
        LongEnum = 0xC, //  # size 4 , # _field_long_enum
        LongFlags = 0xD, //  # size 4 , # _field_long_flags
        WordFlags = 0xE, //  # size 2 , # _field_word_flags
        ByteFlags = 0xF, //  # size 1 , # _field_byte_flags
        ShortPoint2D = 0x10, //  # size 4 , # _field_point_2d -- 2 2bytes?
        ShortRectangle2D = 0x11, //  # size 8 , # _field_rectangle_2d
        RgbPixel32 = 0x12, //  # size 4 , # _field_rgb_color -- hex color codes --- rgb pixel 32 - it's technically only 3 bytes but the final byte is FF
        ArgbPixel32 = 0x13, //  # size 4 , # _field_argb_color --- argb pixel 32
        Real = 0x14, //  # size 4 , # _field_real
        Fraction = 0x15, //  # size 4 , # _field_real_fraction
        RealPoint2D = 0x16, //  # size 8 , # _field_real_point_2d
        RealPoint3D = 0x17, //  # size 12 , # _field_real_point_3d
        RealVector2D = 0x18, //  # size 8 , # _field_real_vector_2d -- 
        RealVector3D = 0x19, //  # size 12 , # _field_real_vector_3d
        RealQuaternion = 0x1A, //  # size 16 , # _field_real_quaternion
        RealEulerAngles2D = 0x1B, //  # size 8 , # _field_real_euler_angles_2d
        RealEulerAngles3D = 0x1C, //  # size 12 , # _field_real_euler_angles_3d
        Plane2D = 0x1D, //  # size 12 , # _field_real_plane_2d
        Plane3D = 0x1E, //  # size 16 , # _field_real_plane_3d
        RealRgbColor = 0x1F, //  # size 12 , # _field_real_rgb_color
        RealARgbColor = 0x20, //  # size 16 , # _field_real_argb_color
        RealHsvColor = 0x21, //  # size 4 , # _field_real_hsv_colo
        RealAhsvColor = 0x22, //  # size 4 , # _field_real_ahsv_color
        ShortBounds = 0x23, //  # size 4 , # _field_short_bounds
        AngleBounds = 0x24, //  # size 8 , # _field_angle_bounds
        RealBounds = 0x25, //  # size 8 , # _field_real_bounds
        FractionBounds = 0x26, //  # size 8 , # _field_real_fraction_bounds
        Unmapped27 = 0x27, //  # size 4 , # ## Not found in any tag type
        Unmapped28 = 0x28, //  # size 4 , # ## Not found in any tag type
        DwordBlockFlags = 0x29, //  # size 4 , # _field_long_block_flags
        WordBlockFlags = 0x2A, //  # size 2 , # _field_word_block_flags
        ByteBlockFlags = 0x2B, //  # size 1 , # _field_byte_block_flags
        CharBlockIndex = 0x2C, //  # size 1 , # _field_char_block_index
        CustomCharBlockIndex = 0x2D, //  # size 1 , # _field_custom_char_block_index
        ShortBlockIndex = 0x2E, //  # size 2 , # _field_short_block_index
        CustomShortBlockIndex = 0x2F, //  # size 2 , # _field_custom_short_block_index
        LongBlockIndex = 0x30, //  # size 4 , # _field_long_block_index
        CustomLongBlockIndex = 0x31, //  # size 4 , # _field_custom_long_block_index
        NotFound32 = 0x32, //  # size 4 , # ## Not found in any tag type
        NotFound33 = 0x33, //  # size 4 , # ## Not found in any tag type
        Pad = 0x34, //  # size 4 , # _field_pad ## variable length
        Skip = 0x35, //  # size 4 , # 'field_skip' ## iirc
        Explanation = 0x36, //  # size 0 , # _field_explanation
        Custom = 0x37, //  # size 0 , # _field_custom
        Struct = 0x38, //  # size 0 , # _field_struct
        Array = 0x39, //  # size 32 , # _field_array
        Unmapped3A = 0x3A, //  # size 4 ,
        EndStruct = 0x3B, //  # size 0 , # ## end of struct or something
        Byte = 0x3C, //  # size 1 , # _field_byte_integer
        Word = 0x3D, //  # size 2 , # _field_word_integer
        Dword = 0x3E, //  # size 4 , # _field_dword_integer
        Qword = 0x3F, //  # size 8 , # _field_qword_integer
        Block = 0x40, //  # size 20 , # _field_block_v2
        TagReference = 0x41, //  # size 28 , # _field_reference_v2
        Data = 0x42, //  # size 24 , # _field_data_v2

        ResourceHandle = 0x43, //  # size 16 , # ok _field_resource_handle

        DataPath = 0x44, //  # size 256, # revisar original 4 --- data path
        Unmapped45 = 0x45, //  # size 4 ,

        NotFound69 = 0x69
    }
    public static class TagCommon
    {
        public static Dictionary<TagElemntTypeV2,int> GROUP_LENGTHS = new()
            {
                { TagElemntTypeV2.Undefined ,0 }, // _field_string
                { TagElemntTypeV2.RootTagInstance ,0 }, // _field_string
                { TagElemntTypeV2.String ,32 }, // _field_string
				{ TagElemntTypeV2.LongString , 256 }, // _field_long_string
				{ TagElemntTypeV2.Mmr3Hash , 4 }, // _field_string_id
				{ TagElemntTypeV2.NotFound03 , 4 }, // ## Not found in any tag type
				{ TagElemntTypeV2.CharIntiger , 1 }, // _field_char_integer
				{ TagElemntTypeV2.Short  , 2 }, // _field_short_integer
				{ TagElemntTypeV2.Long  , 4 }, // _field_long_integer
				{ TagElemntTypeV2.Int64  , 8 }, // _field_int64_integer
				{ TagElemntTypeV2.Angle  , 4 }, // _field_angle
				{ TagElemntTypeV2.StringTag  , 4 }, // _field_tag
				{ TagElemntTypeV2.CharEnum  , 1 }, // _field_char_enum
				{ TagElemntTypeV2.ShortEnum  , 2 }, // _field_short_enum
				{ TagElemntTypeV2.LongEnum  , 4 }, // _field_long_enum
				{ TagElemntTypeV2.LongFlags  , 4 }, // _field_long_flags
				{ TagElemntTypeV2.WordFlags  , 2 }, // _field_word_flags
				{ TagElemntTypeV2.ByteFlags  , 1 }, // _field_byte_flags
				{ TagElemntTypeV2.ShortPoint2D  , 4 }, // _field_point_2d -- 2 2bytes?
				{ TagElemntTypeV2.ShortRectangle2D  , 8 }, // _field_rectangle_2d -> 4 short
				{ TagElemntTypeV2.RgbPixel32  , 4 }, // _field_rgb_color -- hex color codes --- rgb pixel 32 - it's technically only 3 bytes but the final byte is FF
				{ TagElemntTypeV2.ArgbPixel32  , 4 }, // _field_argb_color --- argb pixel 32
				{ TagElemntTypeV2.Real  , 4 }, // _field_real
				{ TagElemntTypeV2.Fraction  , 4 }, // _field_real_fraction
				{ TagElemntTypeV2.RealPoint2D  , 8 }, // _field_real_point_2d
				{ TagElemntTypeV2.RealPoint3D  , 12 }, // _field_real_point_3d
				{ TagElemntTypeV2.RealVector2D  , 8 }, // _field_real_vector_2d -- 
				{ TagElemntTypeV2.RealVector3D  , 12 }, // _field_real_vector_3d
				{ TagElemntTypeV2.RealQuaternion  , 16 }, // _field_real_quaternion
				{ TagElemntTypeV2.RealEulerAngles2D  , 8 }, // _field_real_euler_angles_2d
				{ TagElemntTypeV2.RealEulerAngles3D  , 12 }, // _field_real_euler_angles_3d
				{ TagElemntTypeV2.Plane2D  , 12 }, // _field_real_plane_2d
				{ TagElemntTypeV2.Plane3D  , 16 }, // _field_real_plane_3d
				{ TagElemntTypeV2.RealRgbColor  , 12 }, // _field_real_rgb_color
				{ TagElemntTypeV2.RealARgbColor  , 16 }, // _field_real_argb_color
				{ TagElemntTypeV2.RealHsvColor  , 4 }, // _field_real_hsv_colo
				{ TagElemntTypeV2.RealAhsvColor  , 4 }, // _field_real_ahsv_color
				{ TagElemntTypeV2.ShortBounds  , 4 }, // _field_short_bounds
				{ TagElemntTypeV2.AngleBounds  , 8 }, // _field_angle_bounds
				{ TagElemntTypeV2.RealBounds  , 8 }, // _field_real_bounds
				{ TagElemntTypeV2.FractionBounds  , 8 }, // _field_real_fraction_bounds
				{ TagElemntTypeV2.Unmapped27  , 4 }, // ## Not found in any tag type
				{ TagElemntTypeV2.Unmapped28  , 4 }, // ## Not found in any tag type
				{ TagElemntTypeV2.DwordBlockFlags  , 4 }, // _field_long_block_flags
				{ TagElemntTypeV2.WordBlockFlags  , 2 }, // _field_word_block_flags
				{ TagElemntTypeV2.ByteBlockFlags  , 1 }, // _field_byte_block_flags
				{ TagElemntTypeV2.CharBlockIndex  , 1 }, // _field_char_block_index
				{ TagElemntTypeV2.CustomCharBlockIndex  , 1 }, // _field_custom_char_block_index
				{ TagElemntTypeV2.ShortBlockIndex  , 2 }, // _field_short_block_index
				{ TagElemntTypeV2.CustomShortBlockIndex  , 2 }, // _field_custom_short_block_index
				{ TagElemntTypeV2.LongBlockIndex  , 4 }, // _field_long_block_index
				{ TagElemntTypeV2.CustomLongBlockIndex  , 4 }, // _field_custom_long_block_index
				{ TagElemntTypeV2.NotFound32  , 4 }, // ## Not found in any tag type
				{ TagElemntTypeV2.NotFound33  , 4 }, // ## Not found in any tag type
				{ TagElemntTypeV2.Pad  , 4 }, // _field_pad ## variable length
				{ TagElemntTypeV2.Skip  , 4 }, // 'field_skip' ## iirc
				{ TagElemntTypeV2.Explanation  , 0 }, // _field_explanation
				{ TagElemntTypeV2.Custom  , 0 }, // _field_custom
				{ TagElemntTypeV2.Struct  , 0 }, // _field_struct
				{ TagElemntTypeV2.Array  , 32 }, // _field_array
				{ TagElemntTypeV2.Unmapped3A  , 4 },
                { TagElemntTypeV2.EndStruct  , 0 }, // ## end of struct or something
				{ TagElemntTypeV2.Byte  , 1 }, // _field_byte_integer
				{ TagElemntTypeV2.Word  , 2 }, // _field_word_integer
				{ TagElemntTypeV2.Dword, 4 }, // _field_dword_integer
				{ TagElemntTypeV2.Qword  , 8 }, // _field_qword_integer
				{ TagElemntTypeV2.Block  , 20 }, // _field_block_v2
				{ TagElemntTypeV2.TagReference  , 28 }, // _field_reference_v2
				{ TagElemntTypeV2.Data  , 24 }, // _field_data_v2

				{ TagElemntTypeV2.ResourceHandle  , 16 }, // ok _field_resource_handle

				{ TagElemntTypeV2.DataPath  , 256 },// revisar original 4 --- data path
				{ TagElemntTypeV2.Unmapped45  , 16 },
				{ TagElemntTypeV2.NotFound69  , 0 },
            };
        public static Dictionary<string, int> group_lengths_dict = new()
            {
                { "_0", 32 }, // _field_string
				{ "_1", 256 }, // _field_long_string
				{ "_2", 4 }, // _field_string_id
				{ "_3", 4 }, // ## Not found in any tag type
				{ "_4", 1 }, // _field_char_integer
				{ "_5", 2 }, // _field_short_integer
				{ "_6", 4 }, // _field_long_integer
				{ "_7", 8 }, // _field_int64_integer
				{ "_8", 4 }, // _field_angle
				{ "_9", 4 }, // _field_tag
				{ "_A", 1 }, // _field_char_enum
				{ "_B", 2 }, // _field_short_enum
				{ "_C", 4 }, // _field_long_enum
				{ "_D", 4 }, // _field_long_flags
				{ "_E", 2 }, // _field_word_flags
				{ "_F", 1 }, // _field_byte_flags
				{ "_10", 4 }, // _field_point_2d -- 2 2bytes?
				{ "_11", 8 }, // _field_rectangle_2d -> 4 short
				{ "_12", 4 }, // _field_rgb_color -- hex color codes --- rgb pixel 32 - it's technically only 3 bytes but the final byte is FF
				{ "_13", 4 }, // _field_argb_color --- argb pixel 32
				{ "_14", 4 }, // _field_real
				{ "_15", 4 }, // _field_real_fraction
				{ "_16", 8 }, // _field_real_point_2d
				{ "_17", 12 }, // _field_real_point_3d
				{ "_18", 8 }, // _field_real_vector_2d -- 
				{ "_19", 12 }, // _field_real_vector_3d
				{ "_1A", 16 }, // _field_real_quaternion
				{ "_1B", 8 }, // _field_real_euler_angles_2d
				{ "_1C", 12 }, // _field_real_euler_angles_3d
				{ "_1D", 12 }, // _field_real_plane_2d
				{ "_1E", 16 }, // _field_real_plane_3d
				{ "_1F", 12 }, // _field_real_rgb_color
				{ "_20", 16 }, // _field_real_argb_color
				{ "_21", 4 }, // _field_real_hsv_colo
				{ "_22", 4 }, // _field_real_ahsv_color
				{ "_23", 4 }, // _field_short_bounds
				{ "_24", 8 }, // _field_angle_bounds
				{ "_25", 8 }, // _field_real_bounds
				{ "_26", 8 }, // _field_real_fraction_bounds
				{ "_27", 4 }, // ## Not found in any tag type
				{ "_28", 4 }, // ## Not found in any tag type
				{ "_29", 4 }, // _field_long_block_flags
				{ "_2A", 2 }, // _field_word_block_flags
				{ "_2B", 1 }, // _field_byte_block_flags
				{ "_2C", 1 }, // _field_char_block_index
				{ "_2D", 1 }, // _field_custom_char_block_index
				{ "_2E", 2 }, // _field_short_block_index
				{ "_2F", 2 }, // _field_custom_short_block_index
				{ "_30", 4 }, // _field_long_block_index
				{ "_31", 4 }, // _field_custom_long_block_index
				{ "_32", 4 }, // ## Not found in any tag type
				{ "_33", 4 }, // ## Not found in any tag type
				{ "_34", 4 }, // _field_pad ## variable length
				{ "_35", 4 }, // 'field_skip' ## iirc
				{ "_36", 0 }, // _field_explanation
				{ "_37", 0 }, // _field_custom
				{ "_38", 0 }, // _field_struct
				{ "_39", 32 }, // _field_array
				{ "_3A", 4 },
                { "_3B", 0 }, // ## end of struct or something
				{ "_3C", 1 }, // _field_byte_integer
				{ "_3D", 2 }, // _field_word_integer
				{ "_3E", 4 }, // _field_dword_integer
				{ "_3F", 8 }, // _field_qword_integer
				{ "_40", 20 }, // _field_block_v2
				{ "_41", 28 }, // _field_reference_v2
				{ "_42", 24 }, // _field_data_v2

				{ "_43", 16 }, // ok _field_resource_handle

				{ "_44", 256 },// revisar original 4 --- data path
				{ "_45", 16 },
            };

        public static Dictionary<int, Template?> getSubTaglayoutFrom(string tagLayoutStr, string hash)
        {
            return getSubTaglayoutFrom(TagXmlParseV2.parse_the_mfing_xmls(tagLayoutStr), hash);
        }
        public static Dictionary<int, Template?> getSubTaglayoutFrom(Dictionary<int, Template?>? tagLayout, string hash)
        {
            if (tagLayout == null || string.IsNullOrEmpty(hash))
                return null;
            Dictionary<int, Template?> result = null;
            foreach (var item in tagLayout)
            {
                if (item.Value.E != null && ((item.Value.E.ContainsKey("hashTR0") && item.Value.E["hashTR0"].ToString() == hash) || (item.Value.E.ContainsKey("hash") && item.Value.E["hash"].ToString() == hash)))
                {
                    result = new Dictionary<int, Template?>();
                    result[0] = item.Value;
                    return result;
                }
                if (item.Value is TagLayoutsV2.P) {
                    TagLayoutsV2.P ly = (item.Value as TagLayoutsV2.P);
                    if (ly.B != null && ly.B.Count != 0)
                {
                        result = getSubTaglayoutFrom(ly.B, hash);
                        if (result != null)
                            return result;
                    }
                }
                  

            }
            return null;
        }
    }

}
