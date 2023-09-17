using System.Collections.Generic;

namespace OpenSpartan.Grunt.Models.HaloInfinite
{
    [IsAutomaticallySerializable]
    public class ArmorCorePart
    {
        public int TagId { get; set; }
        public IdentifierName? StyleId { get; set; }
        public List<RegionData>? RegionData { get; set; }
        public CommonItemData? CommonData { get; set; }
    }
}
