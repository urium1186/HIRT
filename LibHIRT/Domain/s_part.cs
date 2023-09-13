using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Domain
{
    public class s_part
    {        
        public int MaterialIndex { set; get; }
        public int TransparentSortingIndex { set; get; }
        public int MaterialPath { set; get; }
        public int IndexStart { set; get; }
        public int IndexCount { set; get; }
        public int PerMeshPartConstantsOffset { set; get; }
        public int PartType { set; get; }
        public int PartFlags { set; get; }
        public int BudgetVertexCount { set; get; }
    }
}
