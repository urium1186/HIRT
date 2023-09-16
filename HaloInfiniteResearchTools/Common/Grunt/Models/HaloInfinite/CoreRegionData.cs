using System.Collections.Generic;


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
