
using LibHIRT.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static LibHIRT.TagReader.Headers.TagHeader;

namespace LibHIRT.TagReader.Headers
{
    
    public class TagHeader
    {
        public struct PreLoadSections {
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
            public int Magic;

            [FieldOffset(4)]
            public int Version;

            [FieldOffset(8)]
            public ulong TypeHash; // this is unic for TagGroups

            [FieldOffset(16)]
            public ulong AssetChecksum;

            [FieldOffset(24)]
            public int DependencyCount;

            [FieldOffset(28)]
            public int DataBlockCount;

            [FieldOffset(32)]
            public int TagStructCount;

            [FieldOffset(36)]
            public int DataReferenceCount;

            [FieldOffset(40)]
            public int TagReferenceCount;

            [FieldOffset(44)]
            public int StringTableSize;

            [FieldOffset(48)]
            public int ZoneSetDataSize;

            [FieldOffset(52)]
            public int unknownDescInfoType;

            [FieldOffset(56)]
            public int HeaderSize;

            [FieldOffset(60)]
            public int DataSize;

            [FieldOffset(64)]
            public int ResourceDataSize; // Section2Size

            [FieldOffset(68)]
            public int Section3Size;

            [FieldOffset(72)]
            public byte HeaderAlignment;

            [FieldOffset(73)]
            public byte TagDataAlightment;

            [FieldOffset(74)]
            public byte ResourceDataAligment;

            [FieldOffset(75)]
            public byte Section3Alightment;

            [FieldOffset(76)]
            public int UnknownProperty4;
        }

        private TagFileHeader tagFileHeaderInst;

        private bool loaded = false;

        public bool Loaded { get => loaded;}
        public TagFileHeader TagFileHeaderInst { get => tagFileHeaderInst; }

        public void read(FileStream f, PreLoadSections preloadSection) {
            readStream(f, preloadSection);
        } 
        
        public void read(MemoryStream f, PreLoadSections preloadSection) {
            readStream(f, preloadSection);    
        }
        public void readStream(Stream f, PreLoadSections preloadSection) {
            byte[] TagHeader = new byte[80];
            f.Seek(0, SeekOrigin.Begin);
            f.Read(TagHeader, 0, 80);

            tagFileHeaderInst = (TagFileHeader)UtilBinaryReader.marshallBinData<TagFileHeader>(TagHeader);
            loaded= true;   
        }

        public byte[] getSesion3Bytes(Stream stream) {
            if (tagFileHeaderInst.Section3Size > 0) {
                var section_3_offset = tagFileHeaderInst.HeaderSize + tagFileHeaderInst.DataSize + tagFileHeaderInst.ResourceDataSize;
                byte[] result = new byte[tagFileHeaderInst.Section3Size];
                stream.Seek(section_3_offset, SeekOrigin.Begin);    
                stream.Read(result, 0, tagFileHeaderInst.Section3Size);
                return result;
            }
            return new byte[0];
        }

        public int DependencyOffset  {
            get => 0x50;
        }

        public int DataBlockOffset {
            get => DependencyOffset + (tagFileHeaderInst.DependencyCount * 0x18);
        }
        
        public int TagStructOffset {
            get => DataBlockOffset + (tagFileHeaderInst.DataBlockCount * 0x10);
        }

        public int DataReferenceOffset {
            get => TagStructOffset + (tagFileHeaderInst.TagStructCount * 0x20);
        }
        public int TagReferenceOffset {
            get => DataReferenceOffset + (tagFileHeaderInst.DataReferenceCount * 0x14);
        }
        public int StringTableOffset {
            get => TagReferenceOffset + (tagFileHeaderInst.TagReferenceCount * 0x10);
        }
        public int zone_set_offset {
            get => StringTableOffset + tagFileHeaderInst.StringTableSize;
        }

    }
}
