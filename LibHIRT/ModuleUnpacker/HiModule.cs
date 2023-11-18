using LibHIRT.Common;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using LibHIRT.Utils;
using Microsoft.Diagnostics.Tracing.Utilities;
using Oodle;
//using OodleSharp;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;
using static LibHIRT.TagReader.TagLayouts;

namespace LibHIRT.ModuleUnpacker
{
    public delegate void FileEntryProcessor(HiModuleFileEntry? fileEntry);
    public class HiModule
    {
        HiModuleHeader _moduleHeader = new HiModuleHeader();
        Dictionary<int,HiModuleFileEntry> _filesIndexLookup = new Dictionary<int, HiModuleFileEntry>();
        List<int> t3es = new List<int>();
        List<HiModuleBlockEntry> blocks = new List<HiModuleBlockEntry>();

        
        private long data_offset;
        private string unpack_dir = "";
        FileStream? _moduleFile;
        BinaryReader  _reader;
        FileStream? _moduleFileHd;
        string _filePath = "";
        private int filesEntryBytesSize;

        #region Properties

        public bool HaveData { get => _moduleHeader.MapRefIndex != -1; }

        #endregion 

        public HiModule(FileStream? moduleFile)
        {
            _moduleFile = moduleFile;
            _reader = new BinaryReader(_moduleFile);
        }

        public HiModule(string? moduleFilepath)
        {
            _moduleFile = new FileStream(moduleFilepath, FileMode.Open, FileAccess.Read);
            _reader = new BinaryReader(_moduleFile);
        }

        public void ReadIn() {
            ReadHeader();
        }

        public void ReadHeader()
        {
            if ((_moduleFile is null) || !_moduleFile.CanRead)
                return;
            if (!_moduleHeader.Loaded)
            {
                _moduleFile.Seek(0, SeekOrigin.Begin);

                byte[] buffer = new byte[72];
                _moduleFile.Read(buffer);
                _moduleHeader.ReadIn(buffer);
                Debug.Assert(_moduleHeader.ResourceCount + _moduleHeader.ResourceIndex == _moduleHeader.FilesCount);
                var len = _moduleFile.Length;
                if (_moduleHeader.MapRefIndex == -1)
                {
                    Debug.Assert(len == 4096);
                }
                Debug.Assert(len >= 4096);
            }
        }

        private HiModuleFileEntry readFileEntryIn(int index = -1)
        {
            HiModuleFileEntry entry = new();
            if (index != -1)
            {
                int pos = _moduleHeader.FileEntrysOffset + (index * _moduleHeader.FileEntrysTypeSize);
                _moduleFile.Seek(pos, SeekOrigin.Begin);
            }
            else {
                return null;
            }
            entry.ReadIn(new BinaryReader(_moduleFile));
            entry.Index = index;
            
            if (_moduleHeader.StringsSize != 0)
            {
                _moduleFile.Seek(_moduleHeader.StringTableOffset + entry.String_offset, SeekOrigin.Begin);
                entry.Path_string = _reader.ReadStringNullTerminated();
            }

            
            if (entry.GlobalTagId1 == -1)
            {
                if (entry.ParentOffResource != -1)
                {
                    Debug.Assert(index >= _moduleHeader.ResourceIndex);
                    Debug.Assert(index < _moduleHeader.ResourceIndex + _moduleHeader.ResourceCount);
                    
                    if (_filesIndexLookup.TryGetValue(entry.ParentOffResource, out var tempFD))
                    {
                        
                        var i_n = tempFD.ResourceFiles.Count;

                        entry.Path_string = tempFD.Path_string + "[" + i_n.ToString() + "_resource_chunk_" + i_n.ToString() + "]";
                        entry.ParentEntryOffResourceRef = tempFD;
                        tempFD.ResourceFiles.Add(entry);
                    };
                }
                else
                {
                    entry.Path_string = entry.TagGroupRev + "\\" + this._moduleHeader.ModuleIntId + "-index-" + index.ToString();
                }
            }
            else
            {
                Debug.Assert(index < _moduleHeader.ResourceIndex);
                if (entry.Path_string=="")
                    entry.Path_string = entry.TagGroupRev + "\\" + Mmr3HashLTU.getMmr3HashFromInt(entry.GlobalTagId1) + "_" + entry.GlobalTagId1 + "." + entry.TagGroupRev;
            }

            

            Debug.Assert(entry.First_block_index + entry.Block_count <= _moduleHeader.BlockCount);
            return entry;
        }

        private HiModuleBlockEntry readBlockEntryIn(int index = -1)
        {
            HiModuleBlockEntry entry = new();
            if (index != -1)
            {
                int pos = _moduleHeader.BlockListOffset + (index * _moduleHeader.BlockListTypeSize);
                _moduleFile.Seek(pos, SeekOrigin.Begin);
            }
            entry.ReadIn(_reader);
            return entry;
        }

        private Int32 readResourceEntryIn(int index = -1)
        {
            Int32 entry = -1;
            if (index != -1)
            {
                int pos = _moduleHeader.ResourceListOffset + (index * _moduleHeader.ResourceListTypeSize);
                _moduleFile.Seek(pos, SeekOrigin.Begin);
            }
            entry = _reader.ReadInt32();
            return entry;
        }

        public HiModuleFileEntry getResourceOfFileAt(HiModuleFileEntry moduleFileEntry, int index)
        {
            int r_index = readResourceEntryIn(moduleFileEntry.First_resource_index + index);
            var fileEntry = readFileEntryIn(r_index);
            if (fileEntry != null && fileEntry.Comp_size != 0)
            {
                if (fileEntry.ParentOffResource != moduleFileEntry.Index)
                {
                    throw new IndexOutOfRangeException("Index out of range."); ;
                }
                return fileEntry;
            }
            throw new IndexOutOfRangeException("Index out of range.");
        }
    }

    
}
