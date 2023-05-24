using LibHIRT.TagReader;

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
