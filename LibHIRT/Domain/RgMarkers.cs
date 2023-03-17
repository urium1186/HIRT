using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Domain
{
    public class RgMarkers
    {
        public string Name { get; set; }
        public int Region_index { get; set; }
        public int Permutation_index { get; set; }
        public int Node_index { get; set; }
        public Flags Flags { get; set; }
        public Vector3 Translation { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Direction { get; set; }
    }
}
