using LibHIRT.Files;
using System.Diagnostics;

namespace LibHIRT.ModuleUnpacker
{
    public class HiModuleFileEntry
    {
        public static int Size = 88; // 0x58
        int resource_count; // 0x00 00
        int parent_file_index; // 0x04 04
        short unk0x08; // 0x08 08
        short block_count; // 0x0A 10
        int first_block_index; // 0x0C 12
        int first_resource_index; // 0x10 16
        char[] tag; // 0x14 4 byte ClassId 20
        string tagGroupRev;
        long local_data_offset; //  int.from_bytes(fb.read(6), byteorder='little')  # 0x18 24
        UInt16 flags;  // 0x1F 31

        int comp_size; // = gf.read_uint32(fb)  # 0x20 32
        int decomp_size; // = gf.read_uint32(fb)  # 0x24 36
        int GlobalTagId; // = gf.read_uint32(fb)  # 0x28 40 GlobalTagId 
        int uncompressedHeaderSize; //  = gf.read_uint32(fb)  # 0x2C 44 UncompressedHeaderSize
        int uncompressedTagDataSize; // = gf.read_uint32(fb)  # 0x30 48 UncompressedTagDataSize
        int uncompressedResourceDataSize; // = gf.read_uint32(fb)  # 0x34 52 UncompressedResourceDataSize
        int uncompressedSection3Size; // 0x38 56
        short TagDataBlockCount; // 0x3A 58 posible int
        private int ResourceBlockCountInt;
        short ResourceBlockCount; // 0x3C 60 posible int
        short ResourceBlockCountPad; // 0x3E 62 posible completo del int siempre 0

        /*int hd1_delta; // = gf.read_uint32(fb)  # 0x38 56 HeaderBlockCount ???
        int header_size; // = gf.read_uint32(fb)  # 0x3C 60 ???*/
        int string_offset; // = gf.read_uint32(fb)  # 0x40 64
        int parent_of_resource;// = gf.read_uint32(fb)  # 0x44 68 data_size????
        byte[] hash; // 80
        int int_unk1;
        int int_unk2;

        /*
int parent_of_resource; // = gf.read_int32(fb)  # 0x44 68, -1 int for one type, not for another
string hash; // = fb.read(0x10).hex().upper()  # 0x48 72 -> 0x58 88
*/
        string path_string = ""; // = gf.offset_to_string(fb, string_table_offset + t1e.string_offset)
        string save_path = ""; // = gf.offset_to_string(fb, string_table_offset + t1e.string_offset)
        HiModule hiModuleRef;

        List<HiModuleFileEntry> _resourceFiles = new List<HiModuleFileEntry>();

        int _index = -1;
        Int64 long_unk1;
        Int64 long_unk2;

        public HiModuleFileEntry(HiModule hiModuleRef)
        {
            this.hiModuleRef = hiModuleRef;
        }

        public HiModuleFileEntry()
        {
            this.hiModuleRef = null;
        }

        public string Path_string { get => path_string; set => path_string = value; }
        public int String_offset { get => string_offset; set => string_offset = value; }
        public int Resource_count { get => resource_count; set => resource_count = value; }
        public int Parent_file_index { get => parent_file_index; set => parent_file_index = value; }
        public short Unk0x08 { get => unk0x08; set => unk0x08 = value; }
        public short Block_count { get => block_count; set => block_count = value; }
        public int First_block_index { get => first_block_index; set => first_block_index = value; }
        public int First_resource_index { get => first_resource_index; set => first_resource_index = value; }
        public char[] Tag { get => tag; set => tag = value; }
        public string TagGroupRev { get => tagGroupRev; set => tagGroupRev = value; }
        public long DataOffset { get => local_data_offset; set => local_data_offset = value; }
        public long InModuleDataOffset { get; set; }
        public UInt16 Flags { get => flags; set => flags = value; }
        public int Comp_size { get => comp_size; set => comp_size = value; }
        public int Decomp_size { get => decomp_size; set => decomp_size = value; }
        public int GlobalTagId1 { get => GlobalTagId; set => GlobalTagId = value; }
        public int UncompressedHeaderSize1 { get => uncompressedHeaderSize; set => uncompressedHeaderSize = value; }
        public int UncompressedTagDataSize1 { get => uncompressedTagDataSize; set => uncompressedTagDataSize = value; }
        public int UncompressedResourceDataSize1 { get => uncompressedResourceDataSize; set => uncompressedResourceDataSize = value; }
        public int UncompressedSection3Size { get => uncompressedSection3Size; set => uncompressedSection3Size = value; }
        public short TagDataBlockCount1 { get => TagDataBlockCount; set => TagDataBlockCount = value; }
        public short ResourceBlockCount1 { get => ResourceBlockCount; set => ResourceBlockCount = value; }
        public short ResourceBlockCountPad1 { get => ResourceBlockCountPad; set => ResourceBlockCountPad = value; }
        public int ParentOffResource { get => parent_of_resource; set => parent_of_resource = value; }
        public byte[] Hash { get => hash; set => hash = value; }
        public string Save_path { get => save_path; set => save_path = value; }
        public HiModule HiModuleRef { get => hiModuleRef; }
        public int Index { get => _index; set => _index = value; }
        public List<HiModuleFileEntry> ResourceFiles { 
            get => _resourceFiles; 
            set => _resourceFiles = value; }

        public ISSpaceFile ParentOffResourceRef { get; internal set; }

        public HiModuleFileEntry ParentEntryOffResourceRef { get; internal set; }

        public void ReadIn(BinaryReader byteStream)
        {
            resource_count = byteStream.ReadInt32(); // 0x00
            
            parent_file_index = byteStream.ReadInt32();  // 0x04
            unk0x08 = byteStream.ReadInt16(); // 0x08
            var bytes_temp = BitConverter.GetBytes(unk0x08);
            block_count = byteStream.ReadInt16(); // 0x0A
            first_block_index = byteStream.ReadInt32(); // 0x0C
            first_resource_index = byteStream.ReadInt32(); // 0x10
            tag = byteStream.ReadChars(4);
            Array.Reverse(tag);// 0x14
            tagGroupRev = new string(tag);
            var bytes = new byte[8];
            bytes[7] = bytes[6] = 0;
            //Array.Copy(byteStream.ReadBytes(6), bytes, 6); 
            //local_data_offset = BitConverter.ToInt64(bytes, 0); // 0x18
            if (bytes_temp[1] != 3 && bytes_temp[1] != 5)
            {
            }
            var temp_dataOffset = (byteStream.ReadUInt64() & 0xffffffffffff);
            local_data_offset = ((long)temp_dataOffset); // 0x18
            Debug.Assert(local_data_offset.ToString() == temp_dataOffset.ToString());
            byteStream.BaseStream.Seek(byteStream.BaseStream.Position - 2, SeekOrigin.Begin);
            flags = byteStream.ReadUInt16(); // 0x1E
            comp_size = byteStream.ReadInt32(); // 0x20
            decomp_size = byteStream.ReadInt32(); // 0x24
            GlobalTagId = byteStream.ReadInt32(); // 0x28
            uncompressedHeaderSize = byteStream.ReadInt32(); // 0x2C
            uncompressedTagDataSize = byteStream.ReadInt32(); // 0x30
            uncompressedResourceDataSize = byteStream.ReadInt32(); // 0x34
            uncompressedSection3Size = byteStream.ReadInt32(); // 0x38

            var temp = byteStream.ReadBytes(4); // 0x3C
            ResourceBlockCountInt = BitConverter.ToInt32(temp, 0);
            ResourceBlockCount = BitConverter.ToInt16(temp, 0); // 0x3C
            ResourceBlockCountPad = BitConverter.ToInt16(temp, 2); // 0x3E
            Debug.Assert(ResourceBlockCount == 0 || ResourceBlockCount == 512 || ResourceBlockCount == 1024 || ResourceBlockCount == 1792); // || ResourceBlockCount == 2048

            Debug.Assert(ResourceBlockCountPad == 0 || ResourceBlockCountPad == 2 || ResourceBlockCountPad == 4 || ResourceBlockCountPad == 8 || ResourceBlockCountPad == 64 || ResourceBlockCountPad == 128 || ResourceBlockCountPad == 256 || ResourceBlockCountPad == 512 || ResourceBlockCountPad == 1024);
            string_offset = byteStream.ReadInt32(); // 0x40
            parent_of_resource = byteStream.ReadInt32(); // 0x44 
            hash = byteStream.ReadBytes(0x10); // 0x4C
            if (tagGroupRev == "bitm") {
                string hashhex = BitConverter.ToString(hash).Replace("-", "");
            }
            
            int_unk1 = BitConverter.ToInt32(hash, 0);
            int_unk2 = BitConverter.ToInt32(hash, 4);
            long_unk1 = BitConverter.ToInt64(hash, 0);
            long_unk2 = BitConverter.ToInt64(hash, 8);
            var dif = decomp_size - (uncompressedHeaderSize + uncompressedTagDataSize + uncompressedResourceDataSize + uncompressedSection3Size);
            Debug.Assert(tagGroupRev == "����" || dif == 0);
            //if (tagGroupRev != "����") {
            if (tagGroupRev == "levl")
            {

                var byt = BitConverter.GetBytes(BitConverter.ToInt64(hash, 0));
                string hex = BitConverter.ToString(byt).Replace("-", "");
            }
            try
            {
                lock (Utils.UIDebug.debugValues)
                {
                    if (!Utils.UIDebug.debugValues.ContainsKey("unk0x08"))
                    {
                        Utils.UIDebug.debugValues["unk0x08"] = new Dictionary<object, List<object>>();
                    }
                    if (!Utils.UIDebug.debugValues["unk0x08"].ContainsKey(bytes_temp[1].ToString()))
                        Utils.UIDebug.debugValues["unk0x08"][bytes_temp[1].ToString()] = new List<object>();
                    if (!Utils.UIDebug.debugValues["unk0x08"][bytes_temp[1].ToString()].Contains(tagGroupRev))
                    {
                        Utils.UIDebug.debugValues["unk0x08"][bytes_temp[1].ToString()].Add(tagGroupRev);
                        Utils.UIDebug.debugValues["unk0x08"][bytes_temp[1].ToString()].Sort();
                    }
                    if (Utils.UIDebug.debugValues["unk0x08"].Count > 300)
                    {

                    }
                }
            }
            catch (Exception ex)
            {


            }

        }
    }
}
