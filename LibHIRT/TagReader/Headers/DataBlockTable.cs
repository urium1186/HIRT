using System.Text;

namespace LibHIRT.TagReader.Headers
{


    public class DataBlock : HeaderTableEntry
    {
        int size;


        short unknownProperty;

        short section;

        long offset;

        long offsetPlus = -1;

        public override int GetSize => 16;

        public short UnknownProperty { get => unknownProperty; set => unknownProperty = value; }
        public int Size { get => size; set => size = value; }
        public short Section { get => section; set => section = value; }
        public long OffsetPlus { get => offsetPlus; set => offsetPlus = value; }
        public long Offset { get => offset; set => offset = value; }

        public DataBlock(Stream input) : base(input)
        {
        }

        public DataBlock(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public DataBlock(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public override void ReadIn()
        {
            size = ReadInt32();
            unknownProperty = ReadInt16();
            section = ReadInt16();
            offset = ReadInt64();
        }


    }

    public class DataBlockTable : HeaderTable<DataBlock>
    {
        public override void readTable(Stream f, TagHeader header)
        {
            if (header.Loaded)
            {

                f.Seek(header.DataBlockOffset, SeekOrigin.Begin);
                for (int i = 0; i < header.TagFileHeaderInst.DataBlockCount; i++)
                {

                    byte[] buffer = new byte[16];

                    f.Read(buffer, 0, 16);

                    MemoryStream stream = new(buffer);
                    DataBlock entry = new(stream);
                    entry.ReadIn();
                    switch (entry.Section)
                    {
                        case 1:
                            entry.OffsetPlus = entry.Offset + header.TagFileHeaderInst.HeaderSize;
                            break;
                        case 2:
                            entry.OffsetPlus = entry.Offset + header.TagFileHeaderInst.HeaderSize + header.TagFileHeaderInst.DataSize;
                            break;
                        case 3:
                            entry.OffsetPlus = entry.Offset + header.TagFileHeaderInst.HeaderSize + header.TagFileHeaderInst.DataSize + header.TagFileHeaderInst.ResourceDataSize;
                            break;
                        default:
                            break;
                    }
                    entries.Add(entry);
                    Console.Write(entries.Count);
                }
            }

        }
        public override DataBlock readTableItem(Stream f, TagHeader header, int pos)
        {
            if (header.Loaded && pos < header.TagFileHeaderInst.DataBlockCount)
            {

                f.Seek(header.DataBlockOffset + pos * 16, SeekOrigin.Begin);


                byte[] buffer = new byte[16];

                f.Read(buffer, 0, 16);

                MemoryStream stream = new(buffer);
                DataBlock entry = new(stream);
                entry.ReadIn();
                switch (entry.Section)
                {
                    case 1:
                        entry.OffsetPlus = entry.Offset + header.TagFileHeaderInst.HeaderSize;
                        break;
                    case 2:
                        entry.OffsetPlus = entry.Offset + header.TagFileHeaderInst.HeaderSize + header.TagFileHeaderInst.DataSize;
                        break;
                    case 3:
                        entry.OffsetPlus = entry.Offset + header.TagFileHeaderInst.HeaderSize + header.TagFileHeaderInst.DataSize + header.TagFileHeaderInst.ResourceDataSize;
                        break;
                    default:
                        break;
                }
                return entry;

            }
            return null;

        }
    }
}
