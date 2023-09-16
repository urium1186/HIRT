using System.Collections.Generic;

namespace OpenSpartan.Grunt.Models.HaloInfinite
{
    [IsAutomaticallySerializable]
    public class ItemOptions
    {
        public bool IsRequired { get; set; }
        public string DefaultOptionPath { get; set; }
        public List<string>? OptionPaths { get; set; }
    }

    [IsAutomaticallySerializable]
    public class ItemOffRegionOptions : ItemOptions
    {

        public List<RegionData>? OffRegionInfo { get; set; }
    }
    [IsAutomaticallySerializable]
    public class ItemHelmetOptions : ItemOptions
    {

        public List<HelmetOptions>? Options { get; set; }
    }
    public class HelmetOptions
    {
        public string HelmetPath { get; set; }
        public ItemOptions? HelmetAttachments { get; set; }
    }


}
