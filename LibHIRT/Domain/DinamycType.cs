using LibHIRT.TagReader;

namespace LibHIRT.Domain
{
    public class DinamycType:IDisposable
    {
        TagInstance _root;
        ITagParseControl _tagParse;
        public DinamycType() { }

        public TagInstance Root { get => _root; set => _root = value; }
        public ITagParseControl TagParse { get => _tagParse; set => _tagParse = value; }

        public TagInstance this[string path]
        {
            get => Root[path];
        }

        public void Dispose()
        {
            _root.Dispose();
            _tagParse.Dispose();
        }
    }
}
