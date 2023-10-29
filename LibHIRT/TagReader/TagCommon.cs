namespace LibHIRT.TagReader
{

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

    public enum TagElemntTypeNew
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
    public class TagCommon
    {
         public static Dictionary<TagElemntTypeNew,int> GROUP_LENGTHS = new()
            {
                { TagElemntTypeNew.Undefined ,0 }, // _field_string
                { TagElemntTypeNew.RootTagInstance ,0 }, // _field_string
                { TagElemntTypeNew.String ,32 }, // _field_string
				{ TagElemntTypeNew.LongString , 256 }, // _field_long_string
				{ TagElemntTypeNew.Mmr3Hash , 4 }, // _field_string_id
				{ TagElemntTypeNew.NotFound03 , 4 }, // ## Not found in any tag type
				{ TagElemntTypeNew.CharIntiger , 1 }, // _field_char_integer
				{ TagElemntTypeNew.Short  , 2 }, // _field_short_integer
				{ TagElemntTypeNew.Long  , 4 }, // _field_long_integer
				{ TagElemntTypeNew.Int64  , 8 }, // _field_int64_integer
				{ TagElemntTypeNew.Angle  , 4 }, // _field_angle
				{ TagElemntTypeNew.StringTag  , 4 }, // _field_tag
				{ TagElemntTypeNew.CharEnum  , 1 }, // _field_char_enum
				{ TagElemntTypeNew.ShortEnum  , 2 }, // _field_short_enum
				{ TagElemntTypeNew.LongEnum  , 4 }, // _field_long_enum
				{ TagElemntTypeNew.LongFlags  , 4 }, // _field_long_flags
				{ TagElemntTypeNew.WordFlags  , 2 }, // _field_word_flags
				{ TagElemntTypeNew.ByteFlags  , 1 }, // _field_byte_flags
				{ TagElemntTypeNew.ShortPoint2D  , 4 }, // _field_point_2d -- 2 2bytes?
				{ TagElemntTypeNew.ShortRectangle2D  , 8 }, // _field_rectangle_2d -> 4 short
				{ TagElemntTypeNew.RgbPixel32  , 4 }, // _field_rgb_color -- hex color codes --- rgb pixel 32 - it's technically only 3 bytes but the final byte is FF
				{ TagElemntTypeNew.ArgbPixel32  , 4 }, // _field_argb_color --- argb pixel 32
				{ TagElemntTypeNew.Real  , 4 }, // _field_real
				{ TagElemntTypeNew.Fraction  , 4 }, // _field_real_fraction
				{ TagElemntTypeNew.RealPoint2D  , 8 }, // _field_real_point_2d
				{ TagElemntTypeNew.RealPoint3D  , 12 }, // _field_real_point_3d
				{ TagElemntTypeNew.RealVector2D  , 8 }, // _field_real_vector_2d -- 
				{ TagElemntTypeNew.RealVector3D  , 12 }, // _field_real_vector_3d
				{ TagElemntTypeNew.RealQuaternion  , 16 }, // _field_real_quaternion
				{ TagElemntTypeNew.RealEulerAngles2D  , 8 }, // _field_real_euler_angles_2d
				{ TagElemntTypeNew.RealEulerAngles3D  , 12 }, // _field_real_euler_angles_3d
				{ TagElemntTypeNew.Plane2D  , 12 }, // _field_real_plane_2d
				{ TagElemntTypeNew.Plane3D  , 16 }, // _field_real_plane_3d
				{ TagElemntTypeNew.RealRgbColor  , 12 }, // _field_real_rgb_color
				{ TagElemntTypeNew.RealARgbColor  , 16 }, // _field_real_argb_color
				{ TagElemntTypeNew.RealHsvColor  , 4 }, // _field_real_hsv_colo
				{ TagElemntTypeNew.RealAhsvColor  , 4 }, // _field_real_ahsv_color
				{ TagElemntTypeNew.ShortBounds  , 4 }, // _field_short_bounds
				{ TagElemntTypeNew.AngleBounds  , 8 }, // _field_angle_bounds
				{ TagElemntTypeNew.RealBounds  , 8 }, // _field_real_bounds
				{ TagElemntTypeNew.FractionBounds  , 8 }, // _field_real_fraction_bounds
				{ TagElemntTypeNew.Unmapped27  , 4 }, // ## Not found in any tag type
				{ TagElemntTypeNew.Unmapped28  , 4 }, // ## Not found in any tag type
				{ TagElemntTypeNew.DwordBlockFlags  , 4 }, // _field_long_block_flags
				{ TagElemntTypeNew.WordBlockFlags  , 2 }, // _field_word_block_flags
				{ TagElemntTypeNew.ByteBlockFlags  , 1 }, // _field_byte_block_flags
				{ TagElemntTypeNew.CharBlockIndex  , 1 }, // _field_char_block_index
				{ TagElemntTypeNew.CustomCharBlockIndex  , 1 }, // _field_custom_char_block_index
				{ TagElemntTypeNew.ShortBlockIndex  , 2 }, // _field_short_block_index
				{ TagElemntTypeNew.CustomShortBlockIndex  , 2 }, // _field_custom_short_block_index
				{ TagElemntTypeNew.LongBlockIndex  , 4 }, // _field_long_block_index
				{ TagElemntTypeNew.CustomLongBlockIndex  , 4 }, // _field_custom_long_block_index
				{ TagElemntTypeNew.NotFound32  , 4 }, // ## Not found in any tag type
				{ TagElemntTypeNew.NotFound33  , 4 }, // ## Not found in any tag type
				{ TagElemntTypeNew.Pad  , 4 }, // _field_pad ## variable length
				{ TagElemntTypeNew.Skip  , 4 }, // 'field_skip' ## iirc
				{ TagElemntTypeNew.Explanation  , 0 }, // _field_explanation
				{ TagElemntTypeNew.Custom  , 0 }, // _field_custom
				{ TagElemntTypeNew.Struct  , 0 }, // _field_struct
				{ TagElemntTypeNew.Array  , 32 }, // _field_array
				{ TagElemntTypeNew.Unmapped3A  , 4 },
                { TagElemntTypeNew.EndStruct  , 0 }, // ## end of struct or something
				{ TagElemntTypeNew.Byte  , 1 }, // _field_byte_integer
				{ TagElemntTypeNew.Word  , 2 }, // _field_word_integer
				{ TagElemntTypeNew.Dword, 4 }, // _field_dword_integer
				{ TagElemntTypeNew.Qword  , 8 }, // _field_qword_integer
				{ TagElemntTypeNew.Block  , 20 }, // _field_block_v2
				{ TagElemntTypeNew.TagReference  , 28 }, // _field_reference_v2
				{ TagElemntTypeNew.Data  , 24 }, // _field_data_v2

				{ TagElemntTypeNew.ResourceHandle  , 16 }, // ok _field_resource_handle

				{ TagElemntTypeNew.DataPath  , 256 },// revisar original 4 --- data path
				{ TagElemntTypeNew.Unmapped45  , 16 },
				{ TagElemntTypeNew.NotFound69  , 0 },
            };
        public static Dictionary<string, long> group_lengths_dict = new()
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
    }

}
