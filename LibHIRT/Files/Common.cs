using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Files
{
    internal class Common
    {
        Dictionary<string, (string, string)> ma_guid_ext_resource = new Dictionary<string, (string, string)>();
        static Common _instance= null;
        private Common() {
            ma_guid_ext_resource["uslg"] = ("51f5b0b22475fcb8", "string_list_resource");
        }
        public static Common Inst { get {
                if (_instance is null)
                    _instance = new Common();
                return _instance;
            } }

        public Dictionary<string, (string, string)> MA_GUID_EXT_RESOURCE { get => ma_guid_ext_resource;}
        /*{
'866fe37c76da2aab': ['resource_data'],
'51f5b0b22475fcb8': ['string_list_resource'],
'b8b51ce9966ad5d0': ['regions_resource'],
'79d743c8ca034820': ['model_animation_resource'],
'7deb4eccbe9aefa2': ['font_file_resource'],
'1f6f971137966e5b': ['facial_animation_resource'],
'9e6c7b4240fa3187': ['resource_data'],
'b06da676339d3deb': ['legacy_bsp_kd_tree'],
'c35a4c51ee160c29': ['tag_resources'],
'83e10bc5509ed314': ['bitmap_resource_handle'],
'75a4781e55634b12': ['sound_bank_sfx_resource', 'sound_bank_resource']
}*/
    }
}
