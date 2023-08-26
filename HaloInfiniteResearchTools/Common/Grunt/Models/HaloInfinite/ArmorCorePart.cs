using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
