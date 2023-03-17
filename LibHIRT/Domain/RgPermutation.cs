using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Domain
{
    public class RgPermutation
    {
        public string Name { get; set; }
        public int MeshIndex { get; set; }
        public int MeshCount { get; set; }
        public string CloneName { get; set; }
        public RgPermutation() { }
    }
}
