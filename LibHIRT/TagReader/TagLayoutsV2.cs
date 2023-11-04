using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader
{
    public class TagLayoutsV2
    {
        public class C: Template
        {
            public TagElemntTypeV2? T { get; set; } // T = type

            public Dictionary<string, object>? E { get; set; } = null;
            /// <summary>
            /// Length of the tagblock
            /// </summary>
            public int S { get; set; } // S = size // length of tagblock

            public string N { get; set; } // N = name // our name for the block 

            public (string, string) xmlPath { get; set; }
            public string G { get; set; }
            public Dictionary<int, Template>? B { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Dictionary<int, string>? STR { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }

        public class P: C
        {   
            public Dictionary<int, Template>? B { get; set; } = null; 
            
        }

        public class E : C
        {
            public Dictionary<int, string>? STR { get; set; } = null; 

        }

        public class F : C
        {
            public Dictionary<int, string>? STR { get; set; } = null; 

        }


    }
}
