using SharpDX.Direct3D11;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Files.FileTypes
{
    public struct ListRef
    {
        public int globalId { get; set; }

        public List<List2Ref> referencesAsso { get; set; } // count2
    };
    public struct List2Ref
    {
        public int globalId0 { get; set; }
        public int globalId1 { get; set; }
    };

    public struct SubEntry
    {
        public int globalId0 { get; set; }
        public int globalId1 { get; set; }
        public short shortC { get; set; }
        public uint count1 { get; set; }
        public ListRef[] references { get; set; } // count1
        public uint count2 { get; set; }
        public List2Ref[] references2 { get; set; } // count2

    };

    public struct EntryRef
    {
        public int globalId { get; set; }
        public uint Count { get; set; }
        public SubEntry[] subentry { get; set; }// Count
    };




    [FileTagGroup("modix")]
    [FileExtension(".index")]
    public class ModuleIndexFile : SSpaceFile
    {
        int count = 0;
        ConcurrentBag<EntryRef> _entries=new ConcurrentBag<EntryRef>();
        
        public ModuleIndexFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {

        }

        public override string FileTypeDisplay => "Index (.index)";

        public ConcurrentBag<EntryRef> Entries { get => _entries; set => _entries = value; }


        public void ReadEntrys()
        {
            BinaryReader temp = Reader;
            if (Reader == null)
            {
                InitReaderFromMemStream();
            }
            
            Reader.BaseStream.Seek(0, SeekOrigin.Begin);
            count = Reader.ReadInt32();
            _entries.Clear();
            Debug.Assert((Parent as ModuleFile).ModuleHeader.ResourceIndex == count + 2 || (Parent as ModuleFile).ModuleHeader.ResourceIndex == count + 1);
            
            for (int i = 0; i < count; i++)
            {
                try
                {
                    EntryRef tempEntry = new EntryRef();
                    ReadEntry(ref tempEntry);
                    _entries.Add(tempEntry);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                
            }

        }

        private void ReadEntry(ref EntryRef entry)
        {
            entry.globalId = Reader.ReadInt32();
            entry.Count = Reader.ReadUInt32();
            entry.subentry = new SubEntry[entry.Count];
            for (int i = 0; i < entry.Count; i++)
            {

                entry.subentry[i].globalId0 = Reader.ReadInt32();
                entry.subentry[i].globalId1 = Reader.ReadInt32();
                entry.subentry[i].shortC = Reader.ReadInt16();
                entry.subentry[i].count1 = Reader.ReadUInt32();
                
                entry.subentry[i].references = new ListRef[entry.subentry[i].count1]; // count1
                for (int j = 0; j < entry.subentry[i].count1; j++)
                {
                    entry.subentry[i].references[j].globalId = Reader.ReadInt32();
                }

                entry.subentry[i].count2 = Reader.ReadUInt32();
                entry.subentry[i].references2 = new List2Ref[entry.subentry[i].count2]; // count1
                for (int j = 0; j < entry.subentry[i].count2; j++)
                {
                    entry.subentry[i].references2[j].globalId0 = Reader.ReadInt32();
                    entry.subentry[i].references2[j].globalId1 = Reader.ReadInt32();
                }
                ; // count2
            }
        }

        public bool haveRefTo(EntryRef entryRef, int globalId) {
            
            foreach (var item in entryRef.subentry)
            {
                foreach (var refers in item.references)
                {
                    if (refers.globalId == globalId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<EntryRef> getAllRefTo(int globalId)
        {
            HashSet<EntryRef> refs = new HashSet<EntryRef>();
            if (_entries.Count==0)
                ReadEntrys();
            foreach (var item in _entries)
            {
                if (item.globalId!= globalId && haveRefTo(item, globalId)) { 
                    refs.Add(item);
                }
            }
            return refs.ToList();
        }
    }
}
