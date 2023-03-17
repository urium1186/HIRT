using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Domain
{
    public class RgRegion
    {
        public string Name { get; set; }
        List<RgPermutation> _permutations;

        public RgRegion() { }
    }
}
