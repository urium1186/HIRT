using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader
{
    public struct ClassFilter {
        public string Hash { get; set; }
        public bool Full { get; set; }
        public string Xml_path { get; set; }
        public Dictionary<string, ClassFilter> onChilds { get; set; }
    }

    public struct PathFilter {
        public string Path { get; set; }
        public bool Full { get; set; }
        public Dictionary<string, PathFilter> onChilds { get; set; }
    }
    public class TagParseControlFiltter
    {
        List<ClassFilter> classFilter = new List<ClassFilter>();
        List<PathFilter> pathFilter = new List<PathFilter>();
        HashSet<string> permitedXmlPaths= new HashSet<string>();

        public List<ClassFilter> ClassFilter { get => classFilter; set => classFilter = value; }
        public List<PathFilter> PathFilter { get => pathFilter; set => pathFilter = value; }
        public HashSet<string> PermitedXmlPaths { get => permitedXmlPaths; set => permitedXmlPaths = value; }
    }
}
