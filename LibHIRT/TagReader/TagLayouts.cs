using LibHIRT.Utils;
using System.Diagnostics;
using System.Xml;

namespace LibHIRT.TagReader
{
    // ###### for anyone interested, check out https://github.com/Lord-Zedd/H5Tags/tree/master/tags // thank you lord zedd
    // ###### its quite useful for mapping out descriptions and stuff
    public class TagLayouts
    {
        public class C : Template
        {
            public TagElemntType? T { get; set; } // T = type
            public Dictionary<int, Template>? B { get; set; } = null; // B = blocks? i forgot what B stands for
            public Dictionary<string, object>? P { get; set; } = null;
            public Dictionary<string, object>? E { get; set; } = null;
            /// <summary>
            /// Length of the tagblock
            /// </summary>
            public int S { get; set; } // S = size // length of tagblock

            public string N { get; set; } // N = name // our name for the block 


            /// <summary>
            /// Set during load, will be used when I add netcode 
            /// </summary>
            public long MemoryAddress { get; set; }

            /// <summary>
            /// The absolute offset from the base address of the tag
            /// eg C2 will resolve to assault_rifle_mp.weapon + C2 
            /// 
            /// This will be recursive so the actual _intValue might be 
            ///		assault_rifle_mp.weapon + C2 + (nested block) 12 + (nested block) 4
            ///		
            /// This will allow us to sync up changes across the server and client without
            /// the need to re-resolve memory addresses.
            /// </summary>
            public string AbsoluteTagOffset { get; set; } // as a string we can append offsets rather than mathmatically adding them

            public (string, string) xmlPath { get; set; }
            public string G { get; set; }
            public Dictionary<int, string> STR { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }

        public class FlagGroupTL : C
        {
            public FlagGroupTL()
            {
                T = TagElemntType.FlagGroup;
            }

            /// <summary>
            /// Amount of bytes for flags
            /// </summary>
            public int A { get; set; }

            /// <summary>
            /// The max bit, if 0 then defaults to A * 8
            /// </summary>
            public int MB { get; set; }
            /// <summary>
            /// String description of the flags
            /// </summary>
            public Dictionary<int, string> STR { get; set; } = new Dictionary<int, string>();
        }
        public class EnumGroupTL : C
        {
            public EnumGroupTL()
            {
                T = TagElemntType.EnumGroup;
            }

            /// <summary>
            /// Amount of bytes for enum
            /// </summary>
            public int A { get; set; }

            /// <summary>
            /// String description of the flags
            /// </summary>
            public Dictionary<int, string> STR { get; set; } = new Dictionary<int, string>();
        }

        /*public static Dictionary<long, C> Tags(string grouptype)
		{
			run_parse r = new run_parse();
			return r.parse_the_mfing_xmls(grouptype);
		}*/

       
    }
}
