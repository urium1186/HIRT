using LibHIRT.Utils;
using Oodle;
//using OodleSharp;
using System.Diagnostics;
using System.Text;
using static LibHIRT.TagReader.TagLayouts;

namespace LibHIRT.ModuleUnpacker
{
    public delegate void FileEntryProcessor(HiModuleFileEntry? fileEntry);
    public class HiModule
    {
        HiModuleHeader moduleHeader = new HiModuleHeader();
        List<HiModuleFileEntry> hiModuleFileEntries = new List<HiModuleFileEntry>();
        List<int> t3es = new List<int>();
        List<HiModuleBlockEntry> blocks = new List<HiModuleBlockEntry>();
        private long data_offset;
        private string unpack_dir = @"D:\HaloInfiniteStuft\Extracted\UnPacked\winter_update\";
        BinaryReader? _binaryReader;
        string _filePath = "";
        private int filesEntryBytesSize;

        public void ReadIn(string filePath)
        {
            _filePath = filePath;
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            _binaryReader = new BinaryReader(fileStream);
            ReadIn(_binaryReader);
        }

        public void ReadIn(BinaryReader binaryReader)
        {
            _binaryReader = binaryReader;
            moduleHeader.ReadIn(binaryReader.ReadBytes(72));
            var file_path = ((System.IO.FileStream)binaryReader.BaseStream).Name;
            var have_hd1 = false;
            FileStream fb_hd1 = null;
            if (File.Exists(file_path + "_hd1"))
            {
                have_hd1 = true;
                //fb_hd1 = new FileStream(file_path + "_hd1", FileMode.Open);

            }
            for (int i = 0; i < moduleHeader.FilesCount; i++)
            {
                HiModuleFileEntry entry = new(this);
                entry.ReadIn(binaryReader);
                hiModuleFileEntries.Add(entry);

                Debug.Assert(entry.First_block_index + entry.Block_count <= moduleHeader.BlockCount);
                //Debug.Assert(entry.ResourceBlockCountPad1 ==0);
                //if (!have_hd1)
                //    Debug.Assert(entry.First_resource_index + entry.Resource_count <= _moduleHeader.ResourceCount);
                if (!UIDebug.debugValues.ContainsKey("Resource_count"))
                    UIDebug.debugValues["Resource_count"] = new Dictionary<object, List<object>>();

                if (!UIDebug.debugValues.ContainsKey("HeaderBlockCount1"))
                    UIDebug.debugValues["HeaderBlockCount1"] = new Dictionary<object, List<object>>();
                UIDebug.debugValues["HeaderBlockCount1"][entry.ResourceBlockCountPad1] = new List<object>();

                if (!UIDebug.debugValues.ContainsKey("TagDataBlockCount"))
                    UIDebug.debugValues["TagDataBlockCount"] = new Dictionary<object, List<object>>();
                UIDebug.debugValues["TagDataBlockCount"][entry.ResourceBlockCountPad1] = new List<object>();
                if (!UIDebug.debugValues.ContainsKey("parent_of_resource"))
                    UIDebug.debugValues["parent_of_resource"] = new Dictionary<object, List<object>>();
                UIDebug.debugValues["parent_of_resource"][entry.ParentOffResource] = new List<object>();

                if (!UIDebug.debugValues.ContainsKey("ResourceBlockCount"))
                {
                    UIDebug.debugValues["ResourceBlockCount"] = new Dictionary<object, List<object>>();

                }
                if (!UIDebug.debugValues.ContainsKey("unk0x08"))
                {
                    UIDebug.debugValues["unk0x08"] = new Dictionary<object, List<object>>();

                }
                UIDebug.debugValues["Resource_count"][entry.Resource_count] = new List<object>();

                UIDebug.debugValues["ResourceBlockCount"][entry.ResourceBlockCount1] = new List<object>();
                UIDebug.debugValues["unk0x08"][entry.Unk0x08] = new List<object>();
                if (entry.First_resource_index + entry.Resource_count > moduleHeader.ResourceCount)
                {
                    if (!UIDebug.debugValues.ContainsKey("resource_index"))
                    {
                        UIDebug.debugValues["resource_index"] = new Dictionary<object, List<object>>();
                        UIDebug.debugValues["resource_index"]["temp"] = new List<object>();
                    }

                    UIDebug.debugValues["resource_index"]["temp"].Add(entry);
                }
                /*if (!ModuleUnpackerClass.idFileList.ContainsKey(entry.GlobalTagId1))
                {
                    //ModuleUnpackerClass.idFileList[entry.GlobalTagId1] = new List<Models.FileDirModel>();
                }
                else {
                    if (entry.GlobalTagId1 != -1)
                    {
                        if (ModuleUnpackerClass.idFileList[entry.GlobalTagId1].Count != 1)
                        {
                        }
                    }
                    else {
                        if (entry.TagGroupMem != "����") { 
                        }
                    }
                }*/
                //ModuleUnpackerClass.idFileList[entry.GlobalTagId1].Add(new Models.FileDirModel(entry,""));

            }


            var pad1 = binaryReader.ReadInt32();
            var pad2 = binaryReader.ReadInt32();
            var t1 = binaryReader.BaseStream.Position;
            Debug.Assert(t1 == moduleHeader.StringTableOffset);
            var t2 = binaryReader.BaseStream.Position;
            var t3 = t2 + moduleHeader.StringsSize;


            if (moduleHeader.Unk0x44 != 0)
            {
            }


            var t4 = binaryReader.BaseStream.Position;
            Debug.Assert(t3 == t4);
            t3es.Clear();
            var test_out_range = false;
            for (int i = 0; i < moduleHeader.ResourceCount; i++)
            {
                var temp_entry = binaryReader.ReadInt32();
                // Debug.Assert(temp_entry <= _moduleHeader.BlockCount);  
                if (temp_entry > moduleHeader.BlockCount)
                {
                    test_out_range = true;

                }
                Debug.Assert(temp_entry <= moduleHeader.ResourceIndex + moduleHeader.ResourceCount);
                Debug.Assert(temp_entry >= moduleHeader.ResourceIndex);
                t3es.Add(temp_entry);
            }
            if (test_out_range)
            {
                Debug.Assert(moduleHeader.Hd1_delta != 0);
                Debug.Assert(moduleHeader.Unk0x18 == 0);
                //Debug.Assert(_moduleHeader.Hd1_delta < binaryReader.BaseStream.Length);

                Debug.Assert(have_hd1);
            }
            else
            {
                if (moduleHeader.Hd1_delta == 0)
                {
                }
                else
                {
                }
            }
            if (have_hd1)
            {
                //Debug.Assert(test_out_range);
            }
            //Debug.Assert(_moduleHeader.Hd1_delta < binaryReader.BaseStream.Length);

            blocks.Clear();
            for (int i = 0; i < moduleHeader.BlockCount; i++)
            {
                HiModuleBlockEntry blockEntry = new HiModuleBlockEntry();
                blockEntry.ReadIn(binaryReader);
                blocks.Add(blockEntry);
            }

            //Debug.Assert(binaryReader.BaseStream.Length >= _moduleHeader.StringsSize + _moduleHeader.Data_size  + _moduleHeader.Hd1_delta);

            var t5 = binaryReader.BaseStream.Position;
            if (t5 >= binaryReader.BaseStream.Length)
            {
            }
            while (binaryReader.ReadByte() == 0)
            {
                if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length - 1)
                    break;
                continue;
            }
            var t6 = binaryReader.BaseStream.Position;
            var t7 = t6 - t5;
            ulong tmp = (ulong)t6;
            data_offset = t6 - 1;
            #region debugs
            if (UIDebug.debugValues.ContainsKey("t7"))
            {
                if (UIDebug.debugValues["t7"].ContainsKey(t7))
                {

                }
                else
                {
                    UIDebug.debugValues["t7"].Add(t7, new List<object>());
                }

            }
            else
            {
                UIDebug.debugValues.Add("t7", new Dictionary<object, List<object>>());
                UIDebug.debugValues["t7"].Add(t7, new List<object>());

            }
            var t8 = binaryReader.BaseStream.Length - t6;
            var t9 = binaryReader.BaseStream.Length - t5;

            if (moduleHeader.Hd1_delta != 0)
            {
                Debug.Assert(0 == moduleHeader.Data_size);
                Debug.Assert(moduleHeader.Unk0x18 == 0);
                Debug.Assert(have_hd1);
            }
            if (moduleHeader.Unk0x18 == 0)
            {
                Debug.Assert(moduleHeader.BlockCount != 0);
                Debug.Assert(moduleHeader.FilesCount != 0);
                if (moduleHeader.Hd1_delta != 0)
                {
                    //Debug.Assert(_moduleHeader.ManifestCount == 0);
                    Debug.Assert(have_hd1);
                }

                else
                {
                    Debug.Assert(!have_hd1);
                    Debug.Assert(moduleHeader.ManifestCount == -1);
                }

            }
            if (moduleHeader.Unk0x18 == 1)
            {
                Debug.Assert(moduleHeader.ManifestCount == 0);
                Debug.Assert(!have_hd1);
                Debug.Assert(moduleHeader.Hd1_delta == 0);

            }
            if (moduleHeader.Unk0x18 == -1)
            {
                //Debug.Assert(_moduleHeader.ManifestCount == 0);
                Debug.Assert(!have_hd1);

            }
            if (moduleHeader.Data_size != 0)
            {
                Debug.Assert(moduleHeader.Hd1_delta == 0);
                //Debug.Assert(_moduleHeader.Unk0x18 == 1);
            }
            if (moduleHeader.Unk0x18 == -1)
            {
                Debug.Assert(moduleHeader.BlockCount == 0);
                Debug.Assert(moduleHeader.FilesCount == 0);
                Debug.Assert(moduleHeader.StringsSize == 0);
                Debug.Assert(moduleHeader.Data_size == 0);
                Debug.Assert(moduleHeader.Hd1_delta == 0);
                Debug.Assert(moduleHeader.ResourceCount == 0);
            }


            if (moduleHeader.Hd1_delta == 0 && moduleHeader.Data_size == 0)
            {
            }
            Debug.Assert(0 == moduleHeader.BlockListOffset);
            Debug.Assert(0 == moduleHeader.ResourceListOffset);
            Debug.Assert(1 == moduleHeader.Unk0x18 || 0 == moduleHeader.Unk0x18 || moduleHeader.Unk0x18 == -1);
            Debug.Assert(-1 == moduleHeader.Unk0x1C || -1 == moduleHeader.Unk0x1C || moduleHeader.Unk0x1C == -1);
            Debug.Assert(moduleHeader.ManifestCount == -1 || moduleHeader.ManifestCount == 0);
            if (moduleHeader.ManifestCount == 0)
            {
            }
            var t10 = t8;
            t10 = t8 + 1 - moduleHeader.Data_size;
            if (t10 != 0)
            {
                //t10 = t8+1 - _moduleHeader.Hd1_delta;
            }
            if (t10 != 0)
            {

            }
            if (t8 != moduleHeader.Data_size - 1)
            {
                if (moduleHeader.Hd1_delta != 0)
                {
                    //Debug.Assert(_moduleHeader.Hd1_delta - 1 == t8);
                }
                else
                {
                    Debug.Assert(moduleHeader.Hd1_delta == 0);
                }
                Debug.Assert(moduleHeader.Data_size == 0);
            }
            #endregion
            //binaryReader.Close();
            //binaryReader.Dispose();
            return;
            for (int i = 0; i < hiModuleFileEntries.Count; i++)
            {
                var fileEntry = hiModuleFileEntries[i];
                if (fileEntry.Save_path.Contains("control_example_2.render_model"))
                {
                    //if (_fileEntry.Save_path.EndsWith("control_example_2.render_model")) {
                    //if (_fileEntry.Save_path.Contains("runtimeloadmetadata")) {
                    var result = TagXmlParse.parse_the_mfing_xmls(fileEntry.TagGroupRev);
                    var fileDataToSave = ReadFile(fileEntry, binaryReader, fb_hd1);
                    //TagFile tagFile= new TagFile();
                    //tagFile.readIn(fileDataToSave); 
                    var faiename = Path.GetFileName(fileEntry.Save_path);
                    var dir_path = Path.GetFullPath(fileEntry.Save_path).Replace(faiename, "");
                    Directory.CreateDirectory(dir_path);
                    var save = new FileStream(fileEntry.Save_path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    save.Write(fileDataToSave.GetBuffer(), 0, (int)fileDataToSave.Length);
                    save.Flush();
                    save.Close();
                }
            }
        }

        public void ReadInHeader(BinaryReader binaryReader)
        {
            _binaryReader = binaryReader;
            _binaryReader.BaseStream.Position = 0;
            if (!moduleHeader.Loaded)
                moduleHeader.ReadIn(binaryReader.ReadBytes(72));
            filesEntryBytesSize = moduleHeader.FilesCount * 88;

        }

        public void ReadInFilesEntrys(BinaryReader binaryReader, FileEntryProcessor? externalOperation = null)
        {
            _binaryReader = binaryReader;
            _binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
            if (!moduleHeader.Loaded)
                moduleHeader.ReadIn(_binaryReader.ReadBytes(72));
            else
                _binaryReader.BaseStream.Seek(72, SeekOrigin.Begin);

            for (int i = 0; i < moduleHeader.FilesCount; i++)
            {
                HiModuleFileEntry entry = new(this);
                entry.ReadIn(_binaryReader);
                hiModuleFileEntries.Add(entry);

                Debug.Assert(entry.First_block_index + entry.Block_count <= moduleHeader.BlockCount);
            }
            Debug.Assert(filesEntryBytesSize + 72 == _binaryReader.BaseStream.Position);
            var pad1 = binaryReader.ReadInt32();
            var pad2 = binaryReader.ReadInt32();
            var stringsBytesArray = _binaryReader.ReadBytes((int)moduleHeader.StringsSize);

            var stringsReader = new BinaryReader(new MemoryStream(stringsBytesArray), Encoding.UTF8);

            for (int i = 0; i < moduleHeader.FilesCount; i++)
            {
                var item = hiModuleFileEntries[i];

                var index = Array.FindIndex(stringsBytesArray, item.String_offset, w => w == 0x00);

                var nextIndex = -1;
                if (i != moduleHeader.FilesCount - 1)
                {
                    nextIndex = hiModuleFileEntries[i + 1].String_offset;
                    if (index != nextIndex - 1)
                    {
                    }
                    Debug.Assert(index == nextIndex - 1);

                    hiModuleFileEntries[i].Path_string = Encoding.ASCII.GetString(stringsBytesArray, item.String_offset, (nextIndex - 1) - item.String_offset);
                    hiModuleFileEntries[i].Save_path = unpack_dir + hiModuleFileEntries[i].Path_string.Replace(" ", "_").Replace(":", "_");
                    //hiModuleFileEntries[i].Save_path = hiModuleFileEntries[i].Path_string.Replace(" ","_spc_").Replace(":", "_dot_");
                }
                else
                {
                    nextIndex = (int)moduleHeader.StringsSize;
                    Debug.Assert(index == moduleHeader.StringsSize - 1);
                    hiModuleFileEntries[i].Path_string = Encoding.ASCII.GetString(stringsBytesArray, item.String_offset, (nextIndex - 1) - item.String_offset);
                    hiModuleFileEntries[i].Save_path = unpack_dir + hiModuleFileEntries[i].Path_string.Replace(" ", "_").Replace(":", "_");
                }
                /*if (!ModuleUnpackerClass.tagGroupFileList.ContainsKey(item.TagGroupMem)) {
                    //ModuleUnpackerClass.tagGroupFileList[item.TagGroupMem] = new List<Models.FileDirModel>();
                }*/
                //ModuleUnpackerClass.tagGroupFileList[item.TagGroupMem].Add(new Models.FileDirModel(item,""));
                //ModuleUnpackerClass.AddFileToDirList(item);
                if (externalOperation != null)
                    externalOperation(item);
            }
            stringsReader.Close();
            stringsReader.Dispose();
        }
        public MemoryStream ReadFile(HiModuleFileEntry file)
        {
            return ReadFile(file, _binaryReader, null);
        }
        public MemoryStream ReadFile(HiModuleFileEntry file, BinaryReader binaryReader, FileStream? fb_hd1)
        {
            long in_file_offset = (file.DataOffset + data_offset);

            var tmp = binaryReader;

            var file_data_offset = in_file_offset;
            if (in_file_offset >= binaryReader.BaseStream.Length)
            {
                if (fb_hd1 == null)
                {

                }
                tmp = new BinaryReader(fb_hd1);
                file_data_offset = in_file_offset - moduleHeader.Hd1_delta;
            }
            MemoryStream decomp_save_data = new MemoryStream();
            if (file.Decomp_size == 0)
                return decomp_save_data;
            if (file.Block_count != 0)
            {
                for (int i = file.First_block_index; i < file.First_block_index + file.Block_count; i++)
                {


                    HiModuleBlockEntry block = blocks[i];
                    if (block.B_compressed)
                    {
                        tmp.BaseStream.Seek(file_data_offset + block.Comp_offset, SeekOrigin.Begin);
                        byte[] data = tmp.ReadBytes(block.Comp_size);
                        //byte[] DecompressedFile = OodleSharp.Oodle.Decompress(data, data.Length, block.Decomp_size);
                        byte[] decomp = OodleWrapper.Decompress(data, data.Length, block.Decomp_size);
                        if (decomp_save_data.Length != block.Decomp_offset)
                            throw new Exception("Skipped data fix");
                        if (decomp == null)
                            throw new Exception("Skipped data fix");
                        else
                            decomp_save_data.Write(decomp);
                    }
                    else
                    {
                        tmp.BaseStream.Seek(file_data_offset + block.Comp_offset, SeekOrigin.Begin);
                        byte[] decomp = tmp.ReadBytes(block.Comp_size);
                        if (decomp_save_data.Length != block.Decomp_offset)
                            throw new Exception("Skipped data fix");
                        decomp_save_data.Write(decomp);
                    }
                }
            }
            else
            {
                if (file.Comp_size == file.Decomp_size)
                    decomp_save_data.Write(tmp.ReadBytes(file.Comp_size));
                else
                {
                    tmp.BaseStream.Seek(file_data_offset, SeekOrigin.Begin);
                    decomp_save_data.Write(OodleWrapper.Decompress(tmp.ReadBytes(file.Comp_size), file.Comp_size, file.Decomp_size));
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

            return decomp_save_data;
        }
    }
}
