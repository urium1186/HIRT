using System.Collections.Generic;

namespace HaloInfiniteResearchTools.Models
{
    public class TreeHierarchicalModel : IFileModel
    {
        public string Name { get; set; }
        public List<TreeHierarchicalModel> Childrens { get; set; }
        public object Value { get; set; }
        public TreeHierarchicalModel()
        {
        }
    }
}
