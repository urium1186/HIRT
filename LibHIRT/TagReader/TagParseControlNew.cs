
using LibHIRT.TagReader.Headers;
using Memory;
using System;
using System.Diagnostics;

using static LibHIRT.TagReader.TagLayouts;

namespace LibHIRT.TagReader
{
    public delegate void OnInstanceReadEventHandler(object sender, EventArgs e);
    public class TagParseControlNew
    {
        public event OnInstanceEventHandler OnInstanceFullLoad;
        public event EventHandler<ITagInstance> OnInstanceLoadEvent;

        struct ResultType
        {
            public ParentTagInstance parent;
            public List<CompoundTagInstance> childs;
        };

        string _filename = "";
        string _tagLayoutTemplate = "";
        Dictionary<long, C?>? _tagLayout;
        TagFile? _tagFile;
        Stream? _f;
        CompoundTagInstance? _rootTagInst;

        public CompoundTagInstance? RootTagInst { get => _rootTagInst; set => _rootTagInst = value; }
        public TagFile? TagFile { get => _tagFile; set => _tagFile = value; }
        public Stream MemoStream { get; private set; }

        public TagParseControlNew(string filename, string tagLayoutTemplate, Dictionary<long, C?>? tagLayout, Stream? f)
        {
            _filename = filename;
            _tagLayoutTemplate = tagLayoutTemplate;
            _tagLayout = tagLayout;
            _f = f;
        }
        public Dictionary<long, C?> getSubTaglayoutFrom(string tagLayoutStr, string hash)
        {
            return getSubTaglayoutFrom(TagXmlParse.parse_the_mfing_xmls(tagLayoutStr), hash);
        }
        public Dictionary<long, C?> getSubTaglayoutFrom(Dictionary<long, C?>? tagLayout, string hash)
        {
            if (tagLayout == null || string.IsNullOrEmpty(hash))
                return null;
            Dictionary<long, C?> result = null;
            foreach (var item in tagLayout)
            {
                if (item.Value.E != null && item.Value.E.ContainsKey("hashTR0") && item.Value.E["hashTR0"].ToString() == hash)
                {
                    result = new Dictionary<long, C?>();
                    result[0] = item.Value;
                    return result;
                }
                if (item.Value.B != null && item.Value.B.Count != 0)
                {
                    result = getSubTaglayoutFrom(item.Value.B, hash);
                    if (result != null)
                        return result;
                }

            }
            return null;
        }

      
    }
}
