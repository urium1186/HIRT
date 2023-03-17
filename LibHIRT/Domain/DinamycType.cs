using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Domain
{
    public class DinamycType
    {
        TagInstance _root;
        public DinamycType() { }

        public TagInstance Root { get => _root; set => _root = value; }

        public TagInstance this[string path]
        {
            get => Root[path];
        }
    }
}
