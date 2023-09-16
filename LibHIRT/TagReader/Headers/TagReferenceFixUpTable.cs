using LibHIRT.Utils;
using System.Diagnostics;
using System.Text;

namespace LibHIRT.TagReader.Headers
{
    public class TagReferenceFixup : HeaderTableEntry
    {
        int fieldBlock;
        int fieldOffset;
        int nameOffset;
        int dependencyIndex;
        TagDependency tagDependency;
        TagStruct parentStruct;
        string strPath = "";


        public TagReferenceFixup(Stream input) : base(input)
        {
        }

        public TagReferenceFixup(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public TagReferenceFixup(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public override int GetSize => 16;

        public int FieldBlock { get => fieldBlock; set => fieldBlock = value; }
        public int FieldOffset { get => fieldOffset; set => fieldOffset = value; }
        public int NameOffset { get => nameOffset; set => nameOffset = value; }
        public int DependencyIndex { get => dependencyIndex; set => dependencyIndex = value; }
        public TagDependency TagDependency { get => tagDependency; set => tagDependency = value; }
        public TagStruct ParentStruct { get => parentStruct; set => parentStruct = value; }
        public string StrPath { get => strPath; set => strPath = value; }


        public override void ReadIn()
        {
            var init_pos = BaseStream.Position;
            fieldBlock = ReadInt32();
            fieldOffset = ReadInt32();
            nameOffset = ReadInt32();
            dependencyIndex = ReadInt32();
            var final_pos = BaseStream.Position;
            Debug.Assert(final_pos - init_pos == GetSize);
        }

    }
    public class TagReferenceFixUpTable : HeaderTable<TagReferenceFixup>
    {
        private DataReferenceTable? dataReferenceTableField;
        private TagDependencyTable? tagDependencyTableField;
        List<string> strings = new List<string>();

        public TagDependencyTable? TagDependencyTableField { get => tagDependencyTableField; set => tagDependencyTableField = value; }
        internal DataReferenceTable DataReferenceTableField { get => dataReferenceTableField; set => dataReferenceTableField = value; }

        public override void readTable(Stream f, TagHeader header)
        {
            f.Seek(header.TagReferenceOffset, SeekOrigin.Begin);
            var init_pos = f.Position;
            byte[] buffer = new byte[header.TagFileHeaderInst.TagReferenceCount * 16];
            f.Read(buffer, 0, header.TagFileHeaderInst.TagReferenceCount * 16);
            var final_pos = f.Position;
            Debug.Assert(final_pos - init_pos == header.TagFileHeaderInst.TagReferenceCount * 16);
            Stream f_stream = new MemoryStream(buffer);
            for (int i = 0; i < header.TagFileHeaderInst.TagReferenceCount; i++)
            {

                TagReferenceFixup entry = new TagReferenceFixup(f_stream);
                entry.ReadIn();
                var temp_offset = (header.TagReferenceOffset + (header.TagFileHeaderInst.TagReferenceCount * 0x10)) + entry.NameOffset;

                //entry.StrPath = UtilBinaryReader.readStringFromOffset(new BinaryReader(f), temp_offset, true);
                entries.Add(entry);
                if (entry.FieldBlock >= dataReferenceTableField.TagStructTableField.Data_block_table.Entries.Count)
                {
                    Debug.Assert(DebugConfig.NoCheckFails);
                }
                var db = dataReferenceTableField.TagStructTableField.Data_block_table.Entries[entry.FieldBlock];
                foreach (var tag_i in dataReferenceTableField.TagStructTableField.Entries)
                {
                    if (tag_i.Field_data_block == db)
                    {
                        entry.ParentStruct = tag_i;
                        break;
                    }
                }

                if (entry.ParentStruct == null)
                {
                    var debug_t = true;
                }

                if (entry.DependencyIndex != -1)
                {
                    entry.TagDependency = tagDependencyTableField.Entries[entry.DependencyIndex];
                    Debug.Assert(entry.NameOffset == entry.TagDependency.Name_offset);
                }
                else
                {
                    var lo = true;
                }
                entry.ParentStruct.L_tag_ref.Add(entry);
            }

            var offset_1 = f.Position;
            var lastPos = offset_1 + header.TagFileHeaderInst.StringTableSize;
            var br = new BinaryReader(f);
            /*while (offset_1 < lastPos)
            {
                strings.Add(UtilBinaryReader.readStringFromOffset(br, offset_1));
                offset_1= br.BaseStream.Position;
            }*/
        }

        public override TagReferenceFixup readTableItem(Stream f, TagHeader header, int pos)
        {
            if (pos >= 0 && pos < header.TagFileHeaderInst.TagReferenceCount)
            {
                f.Seek(header.TagReferenceOffset + pos * 16, SeekOrigin.Begin);
                TagReferenceFixup entry = new TagReferenceFixup(f);
                entry.ReadIn();
                var temp_offset = (header.TagReferenceOffset + (header.TagFileHeaderInst.TagReferenceCount * 0x10)) + entry.NameOffset;

                entry.StrPath = UtilBinaryReader.readStringFromOffset(new BinaryReader(f), temp_offset, true);

                /*if (entry.FieldBlock >= dataReferenceTableField.TagStructTableField.Data_block_table.Entries.Count)
                {
                    Debug.Assert(DebugConfig.NoCheckFails);
                }*/
                var db = dataReferenceTableField.TagStructTableField.Data_block_table.GetTableEntry(f, header, entry.FieldBlock);
                foreach (var tag_i in dataReferenceTableField.TagStructTableField.Entries)
                {
                    if (tag_i.Field_data_block == db)
                    {
                        entry.ParentStruct = tag_i;
                        break;
                    }
                }

                if (entry.ParentStruct == null)
                {
                    var debug_t = true;
                }

                if (entry.DependencyIndex != -1)
                {
                    entry.TagDependency = tagDependencyTableField.GetTableEntry(f, header, entry.DependencyIndex);
                    Debug.Assert(entry.NameOffset == entry.TagDependency.Name_offset);
                }
                else
                {
                    var lo = true;
                }
                entry.ParentStruct.L_tag_ref.Add(entry);
            }
            return null;
        }
    }
}
