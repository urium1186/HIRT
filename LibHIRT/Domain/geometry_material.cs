using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Domain
{
    public class geometry_material
    {
        int _global_id = -1;
        string name = "";

        public int Global_id { get => _global_id; set => _global_id = value; }
        public string Name { get => name; set => name = value; }
    }
}
