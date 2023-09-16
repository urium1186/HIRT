using System.Text;

namespace LibHIRT.TagReader.Headers
{
    public class TagDependency : HeaderTableEntry
    {

        private string tagGroup = "";
        private string tagGroupRev = "";
        private int name_offset = -1;
        private int ref_id_sub = -1;  // AssetID l = 8
        private int ref_id_center = -1;
        private int global_id = -1;
        private int unknown_property = -1;
        private char[] tag;

        public TagDependency(Stream input) : base(input)
        {
        }

        public TagDependency(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public TagDependency(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public override int GetSize => throw new NotImplementedException();

        public string TagGroup { get => tagGroup; set => tagGroup = value; }
        public string TagGroupRev { get => tagGroupRev; set => tagGroupRev = value; }
        public int Name_offset { get => name_offset; set => name_offset = value; }
        public int Ref_id_sub { get => ref_id_sub; set => ref_id_sub = value; }
        public int Ref_id_center { get => ref_id_center; set => ref_id_center = value; }
        public int Global_id { get => global_id; set => global_id = value; }
        public int Unknown_property { get => unknown_property; set => unknown_property = value; }

        public override void ReadIn()
        {
            tag = ReadChars(4);
            tagGroup = new string(tag);
            Array.Reverse(tag);// 0x14
            tagGroupRev = new string(tag);
            name_offset = ReadInt32();
            ref_id_sub = ReadInt32();
            ref_id_center = ReadInt32();
            global_id = ReadInt32();
            unknown_property = ReadInt32();
            if (unknown_property == -1)
            {

            }
        }
    }

    public class TagDependencyTable : HeaderTable<TagDependency>
    {
        public override void readTable(Stream f, TagHeader header)
        {
            f.Seek(header.DependencyOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.TagFileHeaderInst.DependencyCount; i++)
            {
                TagDependency entry = new(f);
                entry.ReadIn();
                entries.Add(entry);
            }
        }

        public override TagDependency readTableItem(Stream f, TagHeader header, int pos)
        {
            if (pos >= 0 && pos < header.TagFileHeaderInst.DependencyCount)
            {
                f.Seek(header.DependencyOffset + pos * 24, SeekOrigin.Begin);
                TagDependency entry = new(f);
                entry.ReadIn();
                return entry;
            }
            return null;
        }
    }
}
