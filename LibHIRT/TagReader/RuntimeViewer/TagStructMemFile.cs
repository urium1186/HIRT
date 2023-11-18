using LibHIRT.Domain;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader.RuntimeViewer
{
    public class TagStructMemFile : IHIRTFile
    {
        public string Datnum;

        public int ObjectId;
        public string ObjectIdStr;

        public string TagGroupMem;

        public long TagData;
        public long ResourceData;

        public string TagTypeDesc;

        public string TagFullName;

        public string TagFile;

        public bool unloaded;

        public Mem M;

        public string Name => $"{ObjectId}-{ObjectIdStr}";

        public string TagGroup => TagGroupMem;

        public long ByteSize => throw new NotImplementedException();

        public string DisplayName
        {
            get
            {
                return $"{TryGetGlobalId().ToString("X")}_{TryGetGlobalId()}";
            }
        }

        public string Extension => TagGroupMem;

        public string Path_string => $"{TagGroupMem}\\{Name}";

        public string InDiskPath => Path_string;

        public DinamycType? Deserialized(TagParseControlFiltter parseControlFiltter = null, bool forceReload = false, EventHandler<ITagInstance> _onDeserialized = null)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
        }

        public HIRTStream GetStream()
        {
            throw new NotImplementedException();
        }

        public void reset()
        {
            
        }

        public int TryGetGlobalId()
        {
            return ObjectId;
        }
    }

}
