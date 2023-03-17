using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Models
{
    public class TreeHierarchicalModel:IFileModel
    {
        public string Name { get; set; }
        public List<TreeHierarchicalModel>  Childrens{ get; set; }
        public object Value { get; set; }
        public TreeHierarchicalModel() { 
        }
    }
}
