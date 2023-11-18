using System.Collections.Generic;

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
