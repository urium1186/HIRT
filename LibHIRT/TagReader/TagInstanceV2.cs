using LibHIRT.TagReader.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader
{
    public class TagInstanceV2 : ITagInstance, INotifyPropertyChanged, IDisposable
    {
        public long GetTagSize => throw new NotImplementedException();

        public object AccessValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string FieldName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool NoAllowEdit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Template TagDef => throw new NotImplementedException();

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public TagInstance GetObjByPath(string path)
        {
            throw new NotImplementedException();
        }

        public void ReadIn(BinaryReader f, TagHeader? header = null)
        {
            throw new NotImplementedException();
        }

        public void ReadIn(TagHeader? header = null)
        {
            throw new NotImplementedException();
        }

        public string ToJson()
        {
            throw new NotImplementedException();
        }

        public void WriteIn(Stream f, long offset = -1, TagHeader? header = null)
        {
            throw new NotImplementedException();
        }
    }
}
