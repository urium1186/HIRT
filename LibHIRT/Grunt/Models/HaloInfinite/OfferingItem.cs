using OpenSpartan.Grunt.Models;

namespace LibHIRT.Grunt.Models.HaloInfinite
{
    [IsAutomaticallySerializable]
    public class OfferingItem
    {
        public int Amount { get; set; }
        public string ItemPath { get; set; }
        public string ItemType { get; set; }

    }
}
