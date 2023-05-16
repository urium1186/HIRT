using LibHIRT.TagReader.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.ModuleUnpacker
{
    enum HiModuleType { 
        NoDotaFile = -1,
        Normal = 0,
        NoExtedHD = 1,
    }
    internal class HiModuleHeader
    {
        //private byte[] moduleHeaderBytes;
        //private string moduleHeaderHex;
        List<object> extra = new List<object>();
        private bool _loaded = false;

        public string Magic { get; set; } // 4 byte
        public int Version { get; set; } // 
        public long ModuleId { get; set; }
        public int FilesCount { get; set; }
        public int ManifestCount { get; set; }

        private int unk0x18;
        private int unk0x1C;

        public int ResourceIndex { get; set; }
        public uint StringsSize { get; set; }
        public uint ResourceCount { get; set; }
        public uint BlockCount { get; set; }

        private uint unk0x30;
        private uint unk0x34;
        private Int64 hd1_delta;
        //private uint unk0x3C;
        //private ulong unk0x38_E;
        private int data_size;
        private uint data_size_u;
        private ulong data_size_E;
        private uint unk0x44;
        private ulong _dataOffset;

        public int ResourceListOffset { get => (int)(StringTableOffset + StringsSize); }
        public int ResourceListSize { get => (int)(ResourceCount * 4); }
        public int ResourceListTypeSize { get =>  4; }
        public int BlockListOffset { get => ResourceListOffset + ResourceListSize; }
        public int BlockListSize { get => (int)(BlockCount * 20); }
        public int BlockListTypeSize { get =>  20; }
        public int FileDataOffset { get; set; }
        //public int FileDataSize { get => 0; }
        public int FileEntrysOffset { get => (72); }
        public int FileEntrysSize { get => (FilesCount * 88); }
        public int FileEntrysTypeSize { get => (88); }
        public int StringTableOffset { get => FileEntrysSize + 72 + 8; }
        
        
        public Dictionary<int, string> Strings { get; set; }
        public int Data_size { get => data_size; set => data_size = value; }
        public uint Unk0x30 { get => unk0x30; set => unk0x30 = value; }
        public uint Unk0x34 { get => unk0x34; set => unk0x34 = value; }
        public Int64 Hd1_delta { get => hd1_delta; set => hd1_delta = value; }
        //public uint Unk0x3C { get => unk0x3C; set => unk0x3C = _intValue; }
        public uint Unk0x44 { get => unk0x44; set => unk0x44 = value; }
        public int Unk0x18 { get => unk0x18; set => unk0x18 = value; }
        public int Unk0x1C { get => unk0x1C; set => unk0x1C = value; }
        public bool Loaded { get => _loaded; }
        public ulong DataOffset { get => _dataOffset; set => _dataOffset = value; }

        public void PrintInfo()
        {
            Console.WriteLine("Header: " + Magic);
            Console.WriteLine("Version: " + Version);
            Console.WriteLine("ModuleId: " + ModuleId);
            Console.WriteLine("Item Count: " + FilesCount);
            Console.WriteLine("Manifest Count: " + ManifestCount);
            Console.WriteLine("Resource Index: " + ResourceIndex);
            Console.WriteLine("Strings Size: " + StringsSize);
            Console.WriteLine("Resource Count: " + ResourceCount);
            Console.WriteLine("Block Count: " + BlockCount);
            Console.WriteLine();
            Console.WriteLine("String Table Offset: 0x" + StringTableOffset.ToString("X8"));
            Console.WriteLine("Resource List Offset: 0x" + ResourceListOffset.ToString("X8"));
            Console.WriteLine("Block List Offset: 0x" + BlockListOffset.ToString("X8"));
            Console.WriteLine("File TagData Offset: 0x" + FileDataOffset.ToString("X8"));
        }

        public void ReadIn(byte[] ModuleHeader) {
            _loaded = false;
            // size 72 
            //moduleHeaderBytes = ModuleHeader;
            //moduleHeaderHex = BitConverter.ToString(moduleHeaderBytes).Replace("-", "");
            Magic = Encoding.ASCII.GetString(ModuleHeader, 0, 4);
            Version = BitConverter.ToInt32(ModuleHeader, 4);
            ModuleId = BitConverter.ToInt64(ModuleHeader, 8);
            extra.Add(BitConverter.ToInt32(ModuleHeader, 8));
            extra.Add(BitConverter.ToInt32(ModuleHeader, 12));
            FilesCount = BitConverter.ToInt32(ModuleHeader, 16);
            ManifestCount = BitConverter.ToInt32(ModuleHeader, 20);
            Debug.Assert(ManifestCount == -1 || ManifestCount == 0);
            unk0x18 = BitConverter.ToInt32(ModuleHeader, 24);
            Debug.Assert(unk0x18 == -1 || unk0x18 == 0 || unk0x18 == 1);

            Debug.Assert((ManifestCount == -1 && (unk0x18 == 0 || unk0x18 == -1)) || (ManifestCount == 0 && unk0x18 == 1));

            unk0x1C = BitConverter.ToInt32(ModuleHeader, 28);
            Debug.Assert(unk0x1C == -1);

            ResourceIndex = BitConverter.ToInt32(ModuleHeader, 32);
            StringsSize = BitConverter.ToUInt32(ModuleHeader, 36);
            ResourceCount = BitConverter.ToUInt32(ModuleHeader, 40);
            BlockCount = BitConverter.ToUInt32(ModuleHeader, 44);
            unk0x30 = BitConverter.ToUInt32(ModuleHeader, 48);
            //Debug.Assert(unk0x30 == 3824403791);//4292057254
            unk0x34 = BitConverter.ToUInt32(ModuleHeader, 52);
            //Debug.Assert(unk0x34 == 2851008788);//807257279
            var a = BitConverter.ToUInt64(ModuleHeader, 48);
            //Debug.Assert(12244989508893001039 == a);//3467143617055004838

            hd1_delta = BitConverter.ToInt64(ModuleHeader, 56);
            //unk0x38_E = BitConverter.ToUInt64(ModuleHeader, 56);
            //Debug.Assert(hd1_delta == unk0x38_E);
            //unk0x3C = BitConverter.ToUInt32(ModuleHeader, 60);
            //Debug.Assert(unk0x3C == 0);

            //hd1_delta = BitConverter.ToUInt32(ModuleHeader, 60);
            //Debug.Assert(hd1_delta == 0);

            data_size = BitConverter.ToInt32(ModuleHeader, 64);
            data_size_u = BitConverter.ToUInt32(ModuleHeader, 64);
            data_size_E = BitConverter.ToUInt64(ModuleHeader, 64);
            Debug.Assert(data_size_u == data_size_E && data_size == data_size_u);
            unk0x44 = BitConverter.ToUInt32(ModuleHeader, 68);
            Debug.Assert(unk0x44 == 0);

            Debug.Assert((hd1_delta == 0 && data_size == 0) || (hd1_delta == 0 && data_size != 0) || (hd1_delta != 0 && data_size == 0));


            UInt64 tmp = (ulong)(BlockListOffset + BlockListSize);
            _dataOffset = tmp;
            if ((tmp & 0xfff) == 0)
            {
                _dataOffset = tmp;
                Debug.Assert(Unk0x18 == -1);
                Debug.Assert(tmp == 4095);
            }
            else
            {
                _dataOffset = (tmp & 0xfffffffffffff000) + 0x1000; // min file size 4096; a file with size of 4096 is a no data file Unk0x18 = -1
                if (_dataOffset < 4096)
                    Debug.Assert(Unk0x18 == -1);
                else
                    if (_dataOffset == 4096 && tmp == 80)
                        Debug.Assert(Unk0x18 == -1);
                    else
                        Debug.Assert(Unk0x18 != -1);
            }
            if (Unk0x18 == -1)
            {
                Debug.Assert(BlockCount == 0);
                Debug.Assert(FilesCount == 0);
                Debug.Assert(StringsSize == 0);
                Debug.Assert(Data_size == 0);
                Debug.Assert(Hd1_delta == 0);
                Debug.Assert(ResourceCount == 0);
                Debug.Assert(ManifestCount == -1);
            }
            else {
                Debug.Assert(_dataOffset >= 4096);
            }
            Debug.Assert((ManifestCount == -1) || (ManifestCount == 0 && Hd1_delta == 0 && unk0x18 == 1));
            Debug.Assert((ManifestCount == -1 && ((unk0x18 == -1) || (unk0x18 == 0 && (Hd1_delta != 0 || Data_size!=0 || BlockCount != 0)))) || (ManifestCount == 0));
            if (Unk0x18 == 0)
            {
                Debug.Assert(BlockCount != 0);    
                Debug.Assert(FilesCount != 0);    
            }
            else if (Unk0x18 == 1)
            {
                Debug.Assert(Hd1_delta == 0);
                //Debug.Assert(ResourceCount == 0);
            }

            if (ResourceCount != 0 && BlockCount != 0 && Hd1_delta != 0)
            {
                Debug.Assert(unk0x18 == 0);
            }
            if (Hd1_delta != 0)
            {
                Debug.Assert(ResourceCount != 0);
                
            }
            if (ResourceCount == 0 && BlockCount != 0)
            {
                
                Debug.Assert(Hd1_delta == 0);
            }

            //Debug.Assert((Data_size == 0 && Hd1_delta == 0));

            _loaded = true;
        }
    
    }
}
