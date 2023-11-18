using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace LibHIRT.Files.FileTypes
{

    public class IncrEntry {
        public uint IntValue { get; set; }
        public uint Index { get; set; }
        public string IntValueStr { get => $"{Index} - {IntValue}"; }

        public override string? ToString()
        {
            return IntValueStr;
        }
    }

    [FileTagGroup("modcr")]
    [FileExtension(".modcr")]
    public class ModuleIncrFile : SSpaceFile
    {
        int count = 0;
        ConcurrentQueue<IncrEntry> _entries=new ConcurrentQueue<IncrEntry>();
        
        public ModuleIncrFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {

        }

        public override string FileTypeDisplay => "incremental (.modcr)";

        public ConcurrentQueue<IncrEntry> Entries { get => _entries; set => _entries = value; }

        public void ReadEntrys()
        {
            BinaryReader temp = Reader;
            if (Reader == null)
            {
                InitReaderFromMemStream();
            }
            
            Reader.BaseStream.Seek(0, SeekOrigin.Begin);
            count = (int)(Reader.BaseStream.Length/4);
            _entries.Clear();
            uint init = 0;
            for (uint i = 0; i < count; i++)
            {
                try
                {
                    uint val =  Reader.ReadUInt32();
                    Debug.Assert(val > init);
                    init = val;
                    _entries.Enqueue(new IncrEntry { IntValue = val, Index = i});
                    
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                
            }

        }
    }
}
