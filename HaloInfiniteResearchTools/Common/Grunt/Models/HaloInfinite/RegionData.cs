﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSpartan.Grunt.Models.HaloInfinite
{
    [IsAutomaticallySerializable]
    public class RegionData
    {
        public IdentifierName? RegionId { get; set; }
        public IdentifierName? PermutationId { get; set; }
        public IdentifierName? StyleIdOverride { get; set; }
    }
}
