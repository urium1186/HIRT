using LibHIRT.Files.Base;
using LibHIRT.ModuleUnpacker;
using System.Diagnostics;
using LibHIRT.Common;
using static LibHIRT.Assertions;
using Oodle;
using LibHIRT.TagReader;
using LibHIRT.Serializers;
using LibHIRT.Utils;
using Oodle.NET;

namespace LibHIRT.Files.FileTypes
{
    [FileSignature("mohd4")]
    [FileExtension(".module")]
    public class ModuleFile : SSpaceContainerFile
    {
        Stream hd1Handle;
        HiModuleHeader _moduleHeader = new HiModuleHeader();
        List<HiModuleFileEntry> hiModuleFileEntries = new List<HiModuleFileEntry>();
        List<int> t3es = new List<int>();
        List<HiModuleBlockEntry> blocks = new List<HiModuleBlockEntry>();
        Dictionary<int, ISSpaceFile> _filesIndexLookup = new Dictionary<int, ISSpaceFile>();
        Dictionary<int, ISSpaceFile> _filesGlobalIdLookup = new Dictionary<int, ISSpaceFile>();
        private void processFileFileEntry(HiModuleFileEntry fileEntry)
        {
            
            var childFile = CreateChildFile(fileEntry.Path_string, 0, fileEntry.Decomp_size, fileEntry.TagGroupRev);
            if (childFile is null)
                return;
            AddChild(childFile);
        }
        #region Properties

        public override string FileTypeDisplay => "Module File (.module)";

        internal HiModuleHeader ModuleHeader { get => _moduleHeader; set => _moduleHeader = value; }
        public Dictionary<int, ISSpaceFile> FilesGlobalIdLookup { get => _filesGlobalIdLookup; set => _filesGlobalIdLookup = value; }

        #endregion

        #region Constructor

        public ModuleFile(string name, HIRTStream baseStream,
          long dataStartOffset, long dataEndOffset,
          ISSpaceFile parent = null)
          : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
        }
        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        void GetHd1Handle()
        {
            try
            {
                if (Reader != null && Reader.BaseStream != null)
                {
                    HIRTDecompressionStream s = Reader.BaseStream as HIRTDecompressionStream;
                    if (s != null) {
                        string path =  s.TryGetFilePath();
                        if (!string.IsNullOrEmpty(path) ) {
                            hd1Handle = File.OpenRead(path+ "_hd1");
                        }
                    }
                }
            }
            catch (Exception)
            {

                hd1Handle = null;
            } 
        }
        public Stream GetFileStreamFromFile(HiModuleFileEntry file) {
            try
            {
                string sub_path = file.Path_string;
                if (sub_path.Contains("resource handle")) {
                    sub_path = sub_path.Replace(":", "_").Replace(" ","_");
                }
                return File.OpenRead(@"D:\HaloInfiniteStuft\Extracted\UnPacked\emulate\M\" + sub_path);
            }
            catch (Exception e)
            {

            }
            return null;
        }

        public Stream GetMemoryStreamFromFile(HiModuleFileEntry file)
        {
            // return GetFileStreamFromFile(file);
            MemoryStream decomp_save_data = new MemoryStream();
            var handle = Reader;
            //file.DataOffset +
            long offset = file.DataOffset;
            //Debug.Assert(file.DataOffset == 0);
            /*
            if (file.DataOffset > _moduleHeader.Data_size + _moduleHeader.Hd1_delta) {

                if (_moduleHeader.Version >= 0x34)
                {
                    if ((file.Flags & 1) != 0)
                    {
                    }
                    else {
                        Debug.Assert(offset <= _moduleHeader.Data_size + _moduleHeader.Hd1_delta);
                        return decomp_save_data;
                    }
                }
                //Debug.Assert(offset <= _moduleHeader.Data_size + _moduleHeader.Hd1_delta);
                //return decomp_save_data;
            }*/

            if (_moduleHeader.Version >= 0x34)
            {
                if ((file.Flags & 1) != 0)
                {
                    if (hd1Handle == null)
                        GetHd1Handle();
                    if (hd1Handle == null)
                    {
                        Assert(hd1Handle != null, "Error extracting %s, no hd1 file for module %s\n");
                        return decomp_save_data;
                    }
                    offset -= _moduleHeader.Hd1_delta;
                    handle = new BinaryReader(hd1Handle);
                }
                else {
                    offset += (long)_moduleHeader.DataOffset;
                }
            }
            else {
                offset += (long)_moduleHeader.DataOffset;
                if ((file.Flags & 1) != 0)
                {
                    if (hd1Handle == null)
                        GetHd1Handle();
                    if (hd1Handle == null)
                    {
                        Assert(hd1Handle != null, "Error extracting %s, no hd1 file for module %s\n");
                        return decomp_save_data;
                    }
                    offset -= _moduleHeader.Hd1_delta;
                    handle = new BinaryReader(hd1Handle);
                }
            }
            file.InModuleDataOffset = offset;
            /*
            var tmp = Reader;
            FileStream? fb_hd1 = null;
            var file_data_offset = offset;
            if (offset >= Reader.BaseStream.Length)
            {
                if (fb_hd1 == null)
                {

                }
                tmp = new BinaryReader(fb_hd1);
                file_data_offset = offset - _moduleHeader.Hd1_delta;
            }*/

            if (file.Decomp_size == 0)
                return decomp_save_data;
            if (file.Block_count != 0)
            {
                for (int i = file.First_block_index; i < file.First_block_index + file.Block_count; i++)
                {


                    HiModuleBlockEntry block = readBlockEntryIn(i);
                    if (block.B_compressed)
                    {
                        if (offset + block.Comp_offset >= handle.BaseStream.Length) {
                            Debug.Assert(DebugConfig.NoCheckFails);
                        }
                        handle.BaseStream.Seek(offset + block.Comp_offset, SeekOrigin.Begin);
                        byte[] data = handle.ReadBytes(block.Comp_size);
                        if (block.Comp_size != data.Length) {
                            Debug.Assert(DebugConfig.NoCheckFails);
                        }
                        //byte[] DecompressedFile = OodleSharp.Oodle.Decompress(data, data.Length, block.Decomp_size);
                        byte[] decomp = OodleWrapper.Decompress(data, block.Comp_size, block.Decomp_size);
                        if (decomp_save_data.Length != block.Decomp_offset)
                            throw new Exception("Skipped data fix");
                        if (decomp == null)
                            throw new Exception("Skipped data fix");
                        else
                            decomp_save_data.Write(decomp);
                    }
                    else
                    {
                        handle.BaseStream.Seek(offset + block.Comp_offset, SeekOrigin.Begin);
                        byte[] decomp = handle.ReadBytes(block.Comp_size);
                        if (decomp_save_data.Length != block.Decomp_offset)
                            throw new Exception("Skipped data fix");
                        decomp_save_data.Write(decomp);
                    }
                }
            }
            else
            {
                handle.BaseStream.Seek(offset, SeekOrigin.Begin);
                if (file.Comp_size == file.Decomp_size)
                    decomp_save_data.Write(handle.ReadBytes(file.Comp_size));
                else
                {
                    decomp_save_data.Write(OodleWrapper.Decompress(handle.ReadBytes(file.Comp_size), file.Comp_size, file.Decomp_size));
                }
                /*
                if t1e.comp_size == t1e.decomp_size:
                    decomp_save_data = tmp.read(t1e.comp_size)
                    #print(t1e.save_path)
                else:
                    tmp.seek(file_data_offset)
                    decomp_save_data = decompressor.decompress(tmp.read(t1e.comp_size), t1e.decomp_size)
                 
                 */
            }
            /*
            decomp_save_data.Seek(0, SeekOrigin.Begin);
            while (decomp_save_data.Position < decomp_save_data.Length) { 
                int b = decomp_save_data.ReadByte();
                if (b != 0 || b == -1)
                    break;
            }
            if (decomp_save_data.Position!= 1) {
                var cant = decomp_save_data.Length - decomp_save_data.Position;
                byte[] bufer = new byte[cant];
                decomp_save_data.Seek(decomp_save_data.Position-1, SeekOrigin.Begin);
                decomp_save_data.Read(bufer);
                decomp_save_data = new MemoryStream(bufer);
            }*/
            
            return decomp_save_data;
        }

        public bool WriteTag(SSpaceFile file) {
            return WriteTag(file.FileMemDescriptor, file.GetStream(), (FileStream)(BaseStream as HIRTDecompressionStream).BaseStream);
        }
        public bool WriteTag(HiModuleFileEntry moduleFileEntry, Stream TagStream, FileStream ModuleStream)
        {
            for (int i = moduleFileEntry.First_block_index; i < moduleFileEntry.First_block_index + moduleFileEntry.Block_count; i++)
            {
                HiModuleBlockEntry block = readBlockEntryIn(i);
                byte[] modifiedBlock = new byte[block.Decomp_size];

                TagStream.Seek(block.Decomp_offset, SeekOrigin.Begin);
                TagStream.Read(modifiedBlock, 0, modifiedBlock.Length);

                byte[] compressedBlock = OodleWrapper.Compress(modifiedBlock, modifiedBlock.Length, OodleLZ_Compressor.Kraken, OodleLZ_CompressionLevel.Optimal5);

                if (modifiedBlock.Length == block.Comp_size)
                {
                    compressedBlock = compressedBlock.Skip(2).ToArray();
                }

                if (compressedBlock.Length <= block.Comp_size)
                {
                    ModuleStream.Seek(moduleFileEntry.InModuleDataOffset +  block.Comp_offset, SeekOrigin.Begin);
                    ModuleStream.Write(compressedBlock, 0, compressedBlock.Length);
                }
                else return false;
            }

            return true;
        }
        protected override void ReadHeader()
        {
            if (!_moduleHeader.Loaded)
                _moduleHeader.ReadIn(Reader.ReadBytes(72));
            var len = BaseStream.Length;
            if (_moduleHeader.Data_size + _moduleHeader.Hd1_delta != 0)
                Debug.Assert(len == ((int)_moduleHeader.DataOffset + _moduleHeader.Data_size + _moduleHeader.Hd1_delta));
            else {
                //Assert(len == 0, "Seek len no equal to file size." + Name);
            }
            if (_moduleHeader.Unk0x18 == -1)
            {
                Debug.Assert(len == 4096);
            }
            Debug.Assert(len >= 4096);
            if (len == 4096)
            {
                Debug.Assert(_moduleHeader.Unk0x18 == -1);
                //Assert(false, "len no data." + Name);
            }
            else { 
                
            }
            if (_moduleHeader.DataOffset < 4096 && _moduleHeader.DataOffset > 80) {
                Debug.Assert(len > 4096);
            }
            if (_moduleHeader.Unk0x18 != -1){
                BaseStream.Seek(_moduleHeader.BlockListOffset + _moduleHeader.BlockListSize, SeekOrigin.Begin);
                while (BaseStream.ReadByte() == 0)
                {
                    if (BaseStream.Position == BaseStream.Length - 1)
                        break;
                    continue;
                }
                Debug.Assert(BaseStream.Position == (long)_moduleHeader.DataOffset+1);
            }
            

        }
        private HiModuleFileEntry readFileEntryIn(int index = -1) {
            HiModuleFileEntry entry = new();
            if (index != -1) {
                int pos = _moduleHeader.FileEntrysOffset + (index * _moduleHeader.FileEntrysTypeSize);
                Reader.BaseStream.Seek(pos, SeekOrigin.Begin);
            }
            entry.ReadIn(Reader);
            entry.Index = index;
            if (_moduleHeader.StringsSize != 0)
            {
                Reader.BaseStream.Seek(_moduleHeader.StringTableOffset + entry.String_offset, SeekOrigin.Begin);
                entry.Path_string = Reader.ReadStringNullTerminated();
            }
            else {
                if (entry.GlobalTagId1 == -1)
                {
                    if (entry.ParentOffResource != -1)
                    {
                        ISSpaceFile tempP;
                        if (_filesIndexLookup.TryGetValue(entry.ParentOffResource, out tempP))
                        {
                            var tempFD = ((SSpaceFile)tempP).FileMemDescriptor;
                            var i_n = tempFD.ResourceFiles.Count;
                           
                            entry.Path_string = tempFD.Path_string + "[" + i_n.ToString() + "_resource_chunk_" + i_n.ToString() + "]";
                            entry.ParentOffResourceRef = tempP;
                            ((SSpaceFile)tempP).FileMemDescriptor.ResourceFiles.Add(entry);
                        };
                    }
                    else {
                        entry.Path_string = entry.TagGroupRev + "\\" + this.ModuleHeader.ModuleId + "-index-"+index.ToString();
                    }
                }
                else {

                    entry.Path_string = entry.TagGroupRev + "\\" + Mmr3HashLTU.getMmr3HashFromInt(entry.GlobalTagId1) +"_"+entry.GlobalTagId1 + "." + entry.TagGroupRev;
                }
                
            }
            
            Debug.Assert(entry.First_block_index + entry.Block_count <= _moduleHeader.BlockCount);
            return entry;
        }

        private HiModuleBlockEntry readBlockEntryIn(int index = -1) { 
            HiModuleBlockEntry entry = new();
            if (index != -1) {
                int pos = _moduleHeader.BlockListOffset + (index * _moduleHeader.BlockListTypeSize);
                Reader.BaseStream.Seek(pos, SeekOrigin.Begin);
            }
            entry.ReadIn(Reader);
            return entry;
        }

        private Int32 readResourceEntryIn(int index = -1)
        {
            Int32 entry = -1;
            if (index != -1)
            {
                int pos = _moduleHeader.ResourceListOffset + (index * _moduleHeader.ResourceListTypeSize);
                Reader.BaseStream.Seek(pos, SeekOrigin.Begin);
            }
            entry = Reader.ReadInt32();
            return entry;
        }
        public ISSpaceFile GetFileByGlobalId(int _gloabalId) {
            
            {
                try
                {
                    return _filesGlobalIdLookup[_gloabalId];
                }
                catch (Exception ex)
                {
                    string module_name = Name.Split("__")[1];    
                    foreach (var item in HIFileContext.FilesModuleGlobalIdLockUp)
                    {
                       string name = item.Key.Split("__")[1];
                        // module_name == name && 
                        if (item.Value.FilesGlobalIdLookup.ContainsKey(_gloabalId)) {
                            var r = item.Value.FilesGlobalIdLookup[_gloabalId];
                            if (r != null && !r.Path_string.Contains(@"__chore\ds__")) {
                                return r;
                            }
                            
                        }
                    } 
                    return null;
                }
                
            }
        }
        protected override void ReadChildren()
        {
            for (int i = 0; i < _moduleHeader.FilesCount; i++)
            {
                
                var entry = readFileEntryIn(i);
                if (entry != null && entry.Comp_size != 0)
                {
                    var childFile = CreateChildFile(entry.Path_string, 0, entry.Decomp_size, entry.TagGroupRev);
                    (childFile as SSpaceFile).FileMemDescriptor = entry;
                    if (!(childFile is null)) {
                        _filesIndexLookup[i] = childFile;
                        if (entry.ParentOffResource != -1)
                        {
                            ISSpaceFile tempP;
                            if (_filesIndexLookup.TryGetValue(entry.ParentOffResource, out tempP))
                            {
                                ((SSpaceFile)tempP).Resource.Add(childFile);
                            };
                        }
                        lock (_filesGlobalIdLookup)
                        {
                            if (!_filesGlobalIdLookup.ContainsKey(entry.GlobalTagId1))
                            {
                                _filesGlobalIdLookup[entry.GlobalTagId1] = childFile;
                                if (Mmr3HashLTU.ForceFillData && (childFile.TagGroup != "����" || (childFile as SSpaceFile).FileMemDescriptor.GlobalTagId1 != -1)) {
                                    try
                                    {
                                        var _deserialized = GenericSerializer.Deserialize(childFile.GetStream(), childFile as IHIRTFile);
                                        _deserialized = null;
                                    }
                                    catch (Exception exp1)
                                    {

                                        
                                    }
                                    
                                }
                            }
                            else
                            {
                                Debug.Assert(true);
                            }
                        }
                        
                        if (entry.Parent_file_index != -1)
                        {
                            if (_filesIndexLookup.ContainsKey(entry.Parent_file_index))
                            {
                                childFile.RefParent = _filesIndexLookup[entry.Parent_file_index];
                                if (!childFile.RefParent.RefChildren.ContainsKey(entry.GlobalTagId1)) {
                                    childFile.RefParent.RefChildren[entry.GlobalTagId1] = childFile;
                                }
                            }
                            else {
                                Debug.Assert(true);
                            }
                        }
                        AddChild(childFile);
                    }
                    

                }
            }
         
            //module.ReadInFilesEntrys(this.Reader, fileProcessor);
            // Create entries
            /*
            for (var i = 0; i < entryCount; i++)
            {
                var name = names[i];
                var offset = offsets[i];
                var size = sizes[i];
            */
            /* If a file is 0 bytes long, it means the file is a dependency,
             * but it isn't present in this current Pck.
             * In this case, we just want to ignore it.
             */
            /*  
            if (size == 0)
                    continue;

                var childFile = CreateChildFile(name, offset, size);
                if (childFile is null)
                    continue;

                AddChild(childFile);
            }*/
        }

        #endregion

        #region Overrides
        protected override ISSpaceFile CreateChildFile(string name, long offset, long size, string signature)
        {
            var dataStartOffset = CalculateTrueChildOffset(offset);
            var dataEndOffset = dataStartOffset + size;

            return SSpaceFileFactory.CreateFile(name, null, dataStartOffset, dataEndOffset, signature, this);
        }
        #endregion

    }
}
