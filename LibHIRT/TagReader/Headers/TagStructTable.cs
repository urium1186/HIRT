using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader.Headers
{
    public struct TagStructInfo {
        public long property_addres = 0;
        public int n_childs = -1;

        public TagStructInfo()
        {
        }
    }
    public class TagStruct : HeaderTableEntry
    {
        TagStructInfo info = new();
        string gUID = "";
        string gUID1 = "";
        string gUID2 = "";
        short typeID = -1;
        TagStructType typeIdTg = TagStructType.Tagblock;
        short unknown_property_bool_0_1 = -1;
        int field_data_block_index = -1;
        int parent_field_data_block_index = -1;
        int field_offset = -1;
        DataBlock? field_data_block = null;
        DataBlock? data_parent = null;
        TagStruct? parent = null;
        List<TagStruct> childs =new List<TagStruct>();
        List<DataReference> l_function = new List<DataReference> ();
        List<TagReferenceFixup> l_tag_ref = new List<TagReferenceFixup>();
        int parent_entry_index = -1;
        int entry_index = -1;
        List<List<byte>> bin_datas = new List<List<byte>>();
        List<string> bin_datas_hex = new List<string>();
        string type_ = "Tagblock";
        string field_name = "";

        public TagStruct(Stream input) : base(input)
        {
        }

        public TagStruct(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public TagStruct(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public override int GetSize => 32;

        public int Field_data_block_index { get => field_data_block_index; set => field_data_block_index = value; }
        public short Unknown_property_bool_0_1 { get => unknown_property_bool_0_1; set => unknown_property_bool_0_1 = value; }
        
        public int Parent_field_data_block_index { get => parent_field_data_block_index; set => parent_field_data_block_index = value; }
        public int Field_offset { get => field_offset; set => field_offset = value; }
        public DataBlock? Field_data_block { get => field_data_block; set => field_data_block = value; }
        public DataBlock? Data_parent { get => data_parent; set => data_parent = value; }
        public TagStruct? Parent { get => parent; set => parent = value; }
        public List<TagStruct> Childs { get => childs; set => childs = value; }
        public int Parent_entry_index { get => parent_entry_index; set => parent_entry_index = value; }
        public int Entry_index { get => entry_index; set => entry_index = value; }
        
        public string Type_ { get => type_; set => type_ = value; }
        public string Field_name { get => field_name; set => field_name = value; }
        public List<List<byte>> Bin_datas { get => bin_datas; set => bin_datas = value; }
        public List<DataReference> L_function { get => l_function; set => l_function = value; }
        public List<TagReferenceFixup> L_tag_ref { get => l_tag_ref; set => l_tag_ref = value; }
        public List<string> Bin_datas_hex { get => bin_datas_hex; set => bin_datas_hex = value; }
        public TagStructType TypeIdTg { get => typeIdTg; set => typeIdTg = value; }

        public override void ReadIn()
        {
            var temp = ReadInt64();
            var temp2 = ReadInt64();
            typeID = ReadInt16();
            typeIdTg = (TagStructType)typeID;
            unknown_property_bool_0_1 = ReadInt16();
            field_data_block_index = ReadInt32();
            parent_field_data_block_index = ReadInt32();
            field_offset = ReadInt32();

        }

        public TagStructInfo readTagStructInfo() {
            var address_to_back = BaseStream.Position;
            var info = new TagStructInfo();
            switch (typeIdTg)
            {
                case TagStructType.Root:
                    info.property_addres = 0;
                    info.n_childs= 1;
                    if (parent_field_data_block_index != -1)
                    {
                        throw new Exception("Root no debe pertenecer a otro campo");
                    }
                    break;
                case TagStructType.Tagblock:
                    info.property_addres = data_parent.OffsetPlus + field_offset;
                    BaseStream.Seek(info.property_addres + 16, SeekOrigin.Begin);
                    info.n_childs = ReadInt32();
                    break;
                case TagStructType.ExternalFileDescriptor:
                    info.property_addres = data_parent.OffsetPlus + field_offset;
                    BaseStream.Seek(info.property_addres + 12, SeekOrigin.Begin);
                    info.n_childs = ReadInt32();
                    Debug.Assert(info.n_childs == 0, "ExternalFileDescriptor no debe tener hijos, ya que estan en archivo aparte");
                    if (info.n_childs !=0)
                    {

                        //throw new Exception("ExternalFileDescriptor no debe tener hijos, ya que estan en archivo aparte");
                    }
                    break;
                case TagStructType.ResourceHandle:
                    info.property_addres = data_parent.OffsetPlus + field_offset;
                    BaseStream.Seek(info.property_addres + 12, SeekOrigin.Begin);
                    info.n_childs = ReadInt32();
                    break;
                case TagStructType.NoDataStartBlock:
                    Debug.Assert(info.n_childs == -1, "NoDataStartBlock significa q no tiene informacion");
                    if (field_data_block_index != -1)
                    {
                        //throw new Exception("NoDataStartBlock significa q no tiene informacion");
                    }
                    info.property_addres = data_parent.OffsetPlus + field_offset;
                    BaseStream.Seek(info.property_addres, SeekOrigin.Begin);
                    info.n_childs = ReadInt32();
                    break;
                default:
                    break;
            }
            BaseStream.Seek(address_to_back, SeekOrigin.Begin);
            return info;

        }

        public List<List<byte>> readDataEntry()
        {
            List<List<byte>> blocks = new List<List<byte>>();
            var pos_on_init = BaseStream.Position;
            info = readTagStructInfo();
            if (typeIdTg == TagStructType.NoDataStartBlock)
            {
                return blocks;
            }
            else if (typeIdTg == TagStructType.ExternalFileDescriptor )
            {
                if (info.n_childs != 0)
                {
                    BaseStream.Seek(pos_on_init, SeekOrigin.Begin);
                    Debug.Assert(info.n_childs == 0, "Error de interpretacion de Datos, ya q son externos");
                    //throw new Exception("Error de interpretacion de Datos, ya q son externos");
                    if (unknown_property_bool_0_1 == 0) {
                        Debug.Assert(field_data_block.Size % info.n_childs == 0);
                    }
                }
                if (unknown_property_bool_0_1 !=0)
                {
                    BaseStream.Seek(field_data_block.OffsetPlus, SeekOrigin.Begin);
                    byte[] buffer = new byte[field_data_block.Size];
                    Read(buffer);
                    blocks.Add(new List<byte>(buffer));
                }
                BaseStream.Seek(pos_on_init, SeekOrigin.Begin);
                return blocks;
            }
            else
            {
                if (info.n_childs==0)
                {
                    if (field_data_block_index != -1) {
                        BaseStream.Seek(pos_on_init, SeekOrigin.Begin);
                        
                        throw new Exception("Si no tiene hijos, el refernce deberia ser -1");
                        
                    }
                        
                    BaseStream.Seek(pos_on_init, SeekOrigin.Begin);
                    return blocks;
                }
                else
                {
                    if (field_data_block == null) {
                        BaseStream.Seek(pos_on_init, SeekOrigin.Begin);
                        if (typeIdTg != TagStructType.ResourceHandle)
                            throw new Exception("Error de algo en archivo, cambiar");
                        else
                        {
                            Debug.Assert(info.n_childs==1);
                            byte[] buffer = new byte[0];
                            blocks.Add(new List<byte>(buffer));
                            return blocks;
                        }
                    }

                        
                    int div = field_data_block.Size / info.n_childs; //quotient is 1
                    int mod = field_data_block.Size % info.n_childs; //remainder is 2
                    if (mod != 0)
                        throw new Exception(" Deberia ser 0 siempre el resto");
                    else { 
                        if (field_data_block.Size == 0)
                            throw new Exception("  Deberia ser moyor q 0, de lo contrario seria un bloke vacio, error division 0");
                        BaseStream.Seek(field_data_block.OffsetPlus, SeekOrigin.Begin);
                        var sub_block_size = div;
                        for (int i = 0; i < info.n_childs; i++)
                        {
                            byte[] buffer = new byte[sub_block_size];
                            Read(buffer);
                            blocks.Add(new List<byte>(buffer));
                        }
                        BaseStream.Seek(pos_on_init, SeekOrigin.Begin);
                        return blocks;
                    }

                }
            }
            return blocks;
        }

        public int getInstanceIndexInParent() {
            if (bin_datas.Count != 0)
            {
                int temp = bin_datas[0].Count;
                if (temp == 0)
                    throw new Exception("Data vacia en conteentry");
                return field_offset / temp;

            }
            return 0;
        }
    }
    public class TagStructTable : HeaderTable<TagStruct>
    {
        DataBlockTable? data_block_table;

        public DataBlockTable? Data_block_table { get => data_block_table; set => data_block_table = value; }

        public override void readTable(Stream f, TagHeader header)
        {
            if (data_block_table == null)
                throw new Exception("Neeed almost a DataBlockTable");
            
            f.Seek(header.TagStructOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.TagFileHeaderInst.TagStructCount; i++)
            {
                TagStruct entry = new TagStruct(f);
                entry.ReadIn();

                if (header.TagFileHeaderInst.DataBlockCount > entry.Field_data_block_index && entry.Field_data_block_index > -1)
                {
                    entry.Field_data_block = (DataBlock?)data_block_table.Entries[entry.Field_data_block_index];
                }
                if (header.TagFileHeaderInst.DataBlockCount > entry.Parent_field_data_block_index && entry.Parent_field_data_block_index > -1)
                {
                    entry.Data_parent = (DataBlock?)data_block_table.Entries[entry.Parent_field_data_block_index];
                    var p_i = getContentEntryByRefIndex(entry.Parent_field_data_block_index);
                    if (p_i != -99999)
                    {
                        entry.Parent_entry_index = p_i;
                        entry.Parent = entries[p_i];
                        entries[p_i].Childs.Add(entry);
                    }
                }

                entry.Entry_index= entries.Count;
                entry.Bin_datas = entry.readDataEntry();
                foreach (var item in entry.Bin_datas)
                {
                    entry.Bin_datas_hex.Add(BitConverter.ToString(item.ToArray()).Replace("-", ""));
                }
                entries.Add(entry); 
            }
        }

        public override TagStruct readTableItem(Stream f, TagHeader header, int pos)
        {
            if (data_block_table == null)
                throw new Exception("Neeed almost a DataBlockTable");

            f.Seek(header.TagStructOffset + pos* 32, SeekOrigin.Begin);
            if  (pos < header.TagFileHeaderInst.TagStructCount)
            {
                TagStruct entry = new TagStruct(f);
                entry.ReadIn();

                if (header.TagFileHeaderInst.DataBlockCount > entry.Field_data_block_index && entry.Field_data_block_index > -1)
                {
                    entry.Field_data_block = (DataBlock?)data_block_table.GetTableEntry(f, header, entry.Field_data_block_index);
                }
                if (header.TagFileHeaderInst.DataBlockCount > entry.Parent_field_data_block_index && entry.Parent_field_data_block_index > -1)
                {
                    entry.Data_parent = (DataBlock?)data_block_table.GetTableEntry(f, header, entry.Parent_field_data_block_index);
                    var p_i = getContentEntryByRefIndex(entry.Parent_field_data_block_index,f, header, pos);
                    if (p_i != -99999)
                    {
                        entry.Parent_entry_index = p_i;
                        entry.Parent = GetTableEntry(f, header,p_i);
                        entry.Parent.Childs.Add(entry);
                    }
                }

                entry.Entry_index =pos+1;
                entry.Bin_datas = entry.readDataEntry();
                foreach (var item in entry.Bin_datas)
                {
                    entry.Bin_datas_hex.Add(BitConverter.ToString(item.ToArray()).Replace("-", ""));
                }
                return entry;
            }
            return null;
        }

        protected int getContentEntryByRefIndex(int ref_index) {
            int count = 0;
            int entry_found = -99999;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Field_data_block_index == ref_index)
                {
                    count++;
                    entry_found = i;
                    return entry_found;
                }
            }
            if (count > 1)
                Console.WriteLine(count);
            return entry_found;
        }

        protected int getContentEntryByRefIndex(int ref_index, Stream f, TagHeader header, int pos)
        {
            int count = 0;
            int entry_found = -99999;
            for (int i = 0; i < pos; i++)
            {
                if (GetTableEntry(f,header,i).Field_data_block_index == ref_index)
                {
                    count++;
                    entry_found = i;
                    return entry_found;
                }
            }
            if (count > 1)
                Console.WriteLine(count);
            return entry_found;
        }
    }
}
