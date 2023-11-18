
using LibHIRT.Utils;
using System.Runtime.InteropServices;

namespace LibHIRT.TagReader.Headers
{

    public class TagHeader
    {
        public struct PreLoadSections
        {
            public bool s_all = true;
            public bool s0 = false;
            public bool s1 = false;
            public bool s2 = false;
            public bool s3 = false;

            public PreLoadSections()
            {
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct TagFileHeader
        {
            [FieldOffset(0)]
            private int magic;

            [FieldOffset(4)]
            private int version;

            [FieldOffset(8)]
            private long typeHash; // this is unic for TagGroups

            [FieldOffset(16)]
            private long assetChecksum;

            [FieldOffset(24)]
            private int dependencyCount;

            [FieldOffset(28)]
            private int dataBlockCount;

            [FieldOffset(32)]
            private int tagStructCount;

            [FieldOffset(36)]
            private int dataReferenceCount;

            [FieldOffset(40)]
            private int tagReferenceCount;

            [FieldOffset(44)]
            private int stringTableSize;

            [FieldOffset(48)]
            private int zoneSetDataSize;

            [FieldOffset(52)]
            private int unknownDescInfoType;

            [FieldOffset(56)]
            private int headerSize;

            [FieldOffset(60)]
            private int dataSize;

            [FieldOffset(64)]
            private int resourceDataSize; // Section2Size

            [FieldOffset(68)]
            private int section3Size;

            [FieldOffset(72)]
            private byte headerAlignment;

            [FieldOffset(73)]
            private byte tagDataAlightment;

            [FieldOffset(74)]
            private byte resourceDataAligment;

            [FieldOffset(75)]
            private byte section3Alightment;

            [FieldOffset(76)]
            private int unknownProperty4;

            public int Magic { get => magic; set => magic = value; }
            public int Version { get => version; set => version = value; }
            public long TypeHash { get => typeHash; set => typeHash = value; }
            public string TypeHashStr { get => typeHash.ToString("X").Length == 15? "0"+typeHash.ToString("X") : typeHash.ToString("X");  }
            public long AssetChecksum { get => assetChecksum; set => assetChecksum = value; }
            public int DependencyCount { get => dependencyCount; set => dependencyCount = value; }
            public int DataBlockCount { get => dataBlockCount; set => dataBlockCount = value; }
            public int TagStructCount { get => tagStructCount; set => tagStructCount = value; }
            public int DataReferenceCount { get => dataReferenceCount; set => dataReferenceCount = value; }
            public int TagReferenceCount { get => tagReferenceCount; set => tagReferenceCount = value; }
            public int StringTableSize { get => stringTableSize; set => stringTableSize = value; }
            public int ZoneSetDataSize { get => zoneSetDataSize; set => zoneSetDataSize = value; }
            public int UnknownDescInfoType { get => unknownDescInfoType; set => unknownDescInfoType = value; }
            public int HeaderSize { get => headerSize; set => headerSize = value; }
            public int DataSize { get => dataSize; set => dataSize = value; }
            public int ResourceDataSize { get => resourceDataSize; set => resourceDataSize = value; }
            public int Section3Size { get => section3Size; set => section3Size = value; }
            public byte HeaderAlignment { get => headerAlignment; set => headerAlignment = value; }
            public byte TagDataAlightment { get => tagDataAlightment; set => tagDataAlightment = value; }
            public byte ResourceDataAligment { get => resourceDataAligment; set => resourceDataAligment = value; }
            public byte Section3Alightment { get => section3Alightment; set => section3Alightment = value; }
            public int UnknownProperty4 { get => unknownProperty4; set => unknownProperty4 = value; }
        }

        private TagFileHeader tagFileHeaderInst;

        private bool loaded = false;

        public bool Loaded { get => loaded; }
        public TagFileHeader TagFileHeaderInst { get => tagFileHeaderInst; }

        public void read(FileStream f, PreLoadSections preloadSection)
        {
            readStream(f, preloadSection);
        }

        public void read(MemoryStream f, PreLoadSections preloadSection)
        {
            readStream(f, preloadSection);
        }
        public void readStream(Stream f, PreLoadSections preloadSection)
        {
            byte[] TagHeader = new byte[80];
            f.Seek(0, SeekOrigin.Begin);
            f.Read(TagHeader, 0, 80);

            tagFileHeaderInst = (TagFileHeader)UtilBinaryReader.marshallBinData<TagFileHeader>(TagHeader);
            loaded = true;
        }

        public byte[] getSesion3Bytes(Stream stream)
        {
            if (tagFileHeaderInst.Section3Size > 0)
            {
                var section_3_offset = tagFileHeaderInst.HeaderSize + tagFileHeaderInst.DataSize + tagFileHeaderInst.ResourceDataSize;
                byte[] result = new byte[tagFileHeaderInst.Section3Size];
                stream.Seek(section_3_offset, SeekOrigin.Begin);
                stream.Read(result, 0, tagFileHeaderInst.Section3Size);
                return result;
            }
            return new byte[0];
        }

        public int DependencyOffset
        {
            get => 0x50;
        }

        public long FileSize
        {
            get => tagFileHeaderInst.HeaderSize + tagFileHeaderInst.DataSize + tagFileHeaderInst.ResourceDataSize + tagFileHeaderInst.Section3Size;
        }


        public int DataBlockOffset
        {
            get => DependencyOffset + (tagFileHeaderInst.DependencyCount * 0x18);
        }

        public int TagStructOffset
        {
            get => DataBlockOffset + (tagFileHeaderInst.DataBlockCount * 0x10);
        }

        public int DataReferenceOffset
        {
            get => TagStructOffset + (tagFileHeaderInst.TagStructCount * 0x20);
        }
        public int TagReferenceOffset
        {
            get => DataReferenceOffset + (tagFileHeaderInst.DataReferenceCount * 0x14);
        }
        public int StringTableOffset
        {
            get => TagReferenceOffset + (tagFileHeaderInst.TagReferenceCount * 0x10);
        }
        public int zone_set_offset
        {
            get => StringTableOffset + tagFileHeaderInst.StringTableSize;
        }

    }
}
