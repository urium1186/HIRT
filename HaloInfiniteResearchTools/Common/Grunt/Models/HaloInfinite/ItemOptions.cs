using OpenSpartan.Grunt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
