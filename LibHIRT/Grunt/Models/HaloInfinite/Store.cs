using OpenSpartan.Grunt.Models;
using OpenSpartan.Grunt.Models.HaloInfinite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Grunt.Models.HaloInfinite
{
    [IsAutomaticallySerializable]
    public class Store
    {
        public string StoreId { get; set; }
        public SkipNode StorefrontExpirationDate { get; set; }
        public string StorefrontDisplayPath { get; set; }
        public List<Offering> Offerings { get; set; }
    }
}
