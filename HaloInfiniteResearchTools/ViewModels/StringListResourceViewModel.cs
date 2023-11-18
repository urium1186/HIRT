using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Common;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(StringListResourceFile))]
    public class StringListResourceViewModel : SSpaceFileViewModel<StringListResourceFile>, IDisposeWithView
    {
        Dictionary<string, string> _stringLookupInfo = new Dictionary<string, string>();
        public StringListResourceViewModel(IServiceProvider serviceProvider, StringListResourceFile file) : base(serviceProvider, file)
        {
        }

        public Dictionary<string, string> StringLookupInfo { get => _stringLookupInfo; set => _stringLookupInfo = value; }

        protected override Task OnInitializing()
        {
            var root = File.Deserialized().Root as ResourceHandle;
            
            if (root is not null) {
                
                {
                    _stringLookupInfo.Clear();
                    Debug.Assert(root.Count == 1);
                    
                    foreach (var item in root)
                    {
                        TagData data = item["string data utf8"] as TagData;
                        if (data is null || data.ByteLengthCount == 0)
                            continue;
                        byte[] buffer = data.ReadBuffer();
                        BinaryReader r = new BinaryReader(new MemoryStream(buffer));

                        ListTagInstance string_lookup_info = item["string lookup info"] as ListTagInstance;
                        if (string_lookup_info is not null)
                        {
                            foreach (var lookup_info in string_lookup_info)
                            {
                                int offset = (int)lookup_info["offset"].AccessValue;
                                string string_id = (lookup_info["string id"] as Mmr3Hash).Str_value;
                                r.BaseStream.Seek(offset, SeekOrigin.Begin);
                                //_stringLookupInfo[string_id] = buffer.ReadStringNullTerminated(offset);
                                _stringLookupInfo[string_id] = r.ReadStringNullTerminated();
                            }
                        }
                    }
                    if (_stringLookupInfo.Count > 0)
                        OnPropertyChanged("StringLookupInfo");

                }
            }
            
            return base.OnInitializing();
        }
    }
}
