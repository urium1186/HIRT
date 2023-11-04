using System.Text;

namespace LibHIRT.TagReader.Headers
{
    public class DataReference : HeaderTableEntry
    {
        int parent_struct_index = -1;
        TagStruct? parent_struct = null;
        int unknown_property = -1;

        int field_data_block_index = -1;
        DataBlock? field_data_block = null;

        int parent_field_data_block_index = -1;
        int field_offset = -1;
        List<byte> bin_data = new List<byte>();
        string bin_data_hex = "";
        bool loaded_bin_data = false;
        string path_file = "";

        public DataReference(Stream input) : base(input)
        {
        }

        public DataReference(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public DataReference(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public override int GetSize => 20;

        public int Parent_struct_index { get => parent_struct_index; set => parent_struct_index = value; }
        public TagStruct? Parent_struct { get => parent_struct; set => parent_struct = value; }
        public int Unknown_property { get => unknown_property; set => unknown_property = value; }
        public int Field_data_block_index { get => field_data_block_index; set => field_data_block_index = value; }
        public DataBlock? Field_data_block { get => field_data_block; set => field_data_block = value; }
        public int Parent_field_data_block_index { get => parent_field_data_block_index; set => parent_field_data_block_index = value; }
        public int Field_offset { get => field_offset; set => field_offset = value; }
        public List<byte> Bin_data { get => bin_data; set => bin_data = value; }
        public string Bin_data_hex { get => bin_data_hex; set => bin_data_hex = value; }
        public bool Loaded_bin_data { get => loaded_bin_data; set => loaded_bin_data = value; }
        public string Path_file { get => path_file; set => path_file = value; }

        public override void ReadIn()
        {
            //path_file = f.name
            parent_struct_index = ReadInt32();
            //Debug.Assert(parent_struct_index<header.tag_struct_count
            unknown_property = ReadInt32();
            //if self.unknown_property != 0:
            //   debug = 1

            field_data_block_index = ReadInt32();
            parent_field_data_block_index = ReadInt32();
            field_offset = ReadInt32();
        }

        public byte[] readBinData()
        {
            var pos_on_init = BaseStream.Position;
            BaseStream.Seek(field_data_block.OffsetPlus, SeekOrigin.Begin);
            byte[] r = ReadBytes(field_data_block.Size);
            //self.bin_data = f.read(self.field_data_block.size)
            BaseStream.Seek(pos_on_init, SeekOrigin.Begin);
            return r;
        }
        public byte[] readBinData(int offset, int len)
        {
            if (field_data_block == null)
                throw new Exception("The TagData Reference need a field_data_block");
            var pos_on_init = BaseStream.Position;
            BaseStream.Seek(field_data_block.OffsetPlus + offset, SeekOrigin.Begin);
            if (len > field_data_block.Size)
                throw new Exception();
            byte[] r = ReadBytes(len);
            //self.bin_data = f.read(self.field_data_block.size)
            BaseStream.Seek(pos_on_init, SeekOrigin.Begin);
            return r;
        }
        /*
             def readBinData(self, f=None, header=None):
            if self.field_data_block_index != -1:
                f_close = False
                if f is None:
                    f = open(self.path_file, 'rb')
                    f_close = True
                pos_on_init = f.tell()
                f.seek(self.field_data_block.offset_plus)
                self.bin_data = f.read(self.field_data_block.size)
                f.seek(pos_on_init)
                if f_close:
                    f.close()
                self.bin_data_hex = self.bin_data.hex()
            self.loaded_bin_data = True
         */
    }

    class DataReferenceTable : HeaderTable<DataReference>
    {
        private TagStructTable? tagStructTableField;
        public bool Read_entry_data { get => read_entry_data; set => read_entry_data = value; }
        public event EventHandler<DataReference> OnReadEntryEvent;

        bool read_entry_data = false;
        internal TagStructTable TagStructTableField { get => tagStructTableField; set => tagStructTableField = value; }

        public override void readTable(Stream f, TagHeader header)
        {
            f.Seek(header.DataReferenceOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.TagFileHeaderInst.DataReferenceCount; i++)
            {
                DataReference entry = new DataReference(f);
                entry.ReadIn();
                entry.Parent_struct = tagStructTableField.Entries[entry.Parent_struct_index];
                if (entry.Field_data_block_index != -1)
                {
                    entry.Field_data_block = tagStructTableField.Data_block_table.Entries[entry.Field_data_block_index];
                    if (read_entry_data)
                    {
                        entry.readBinData();
                    }
                    else
                    {
                        entry.Bin_data = new List<byte>(entry.Field_data_block.Size);
                    }
                }
                entry.IndexOnParent = entry.Parent_struct.L_function.Count;
                entry.Parent_struct.L_function.Add(entry);
                entry.Index = entries.Count;
                entries.Add(entry);

                if (OnReadEntryEvent != null)
                    OnReadEntryEvent.Invoke(this, entry);
            }
        }

        public override DataReference readTableItem(Stream f, TagHeader header, int pos)
        {
            if (pos > 0 && pos < header.TagFileHeaderInst.DataReferenceCount)
            {
                f.Seek(header.DataReferenceOffset + pos * 20, SeekOrigin.Begin);
                DataReference entry = new DataReference(f);
                entry.Index = pos;
                entry.ReadIn();
                if (tagStructTableField.Entries != null && entry.Parent_struct_index < tagStructTableField.Entries.Count)
                {
                    entry.Parent_struct = tagStructTableField.Entries[entry.Parent_struct_index];
                }
                else
                {
                    entry.Parent_struct = tagStructTableField.readTableItem(f, header, entry.Parent_struct_index);
                }

                if (entry.Field_data_block_index != -1)
                {
                    entry.Field_data_block = tagStructTableField.Data_block_table.GetTableEntry(f, header, entry.Field_data_block_index);
                    if (read_entry_data)
                    {
                        entry.readBinData();
                    }
                    else
                    {
                        entry.Bin_data = new List<byte>(entry.Field_data_block.Size);
                    }
                }
                entry.IndexOnParent = entry.Parent_struct.L_function.Count;
                entry.Parent_struct.L_function.Add(entry);
            }
            return null;
        }
    }
}
