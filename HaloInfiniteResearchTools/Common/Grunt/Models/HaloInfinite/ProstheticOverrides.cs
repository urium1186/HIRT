using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSpartan.Grunt.Models.HaloInfinite
{
    [IsAutomaticallySerializable]
    public class ProstheticOverrides
    {
        public List<RegionData>? Full { get; set; }
        public List<RegionData>? Half { get; set; }
        public List<RegionData>? Extremity { get; set; }

    }
}
