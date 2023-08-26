using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OpenSpartan.Grunt.Models.HaloInfinite
{
    [IsAutomaticallySerializable]
    public class CoreRegionData
    {
        public List<RegionData>? BaseRegionData { get; set; }
        public List<RegionData>? BodyTypeSmallOverrides { get; set; }
        public List<RegionData>? BodyTypeLargeOverrides { get; set; }
        public ProstheticOverrides? ProstheticLeftArmOverrides { get; set; }
        public ProstheticOverrides? ProstheticRightArmOverrides { get; set; }
        public ProstheticOverrides? ProstheticLeftLegOverrides { get; set; }
        public ProstheticOverrides? ProstheticRightLegOverrides { get; set; }


    }
}
