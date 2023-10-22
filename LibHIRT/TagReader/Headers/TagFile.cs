using static LibHIRT.TagReader.Headers.TagHeader;

namespace LibHIRT.TagReader.Headers
{
    public class TagFile
    {
        TagHeader tagHeader = new TagHeader();
        TagDependencyTable tagDependencyTable = new TagDependencyTable();
        DataBlockTable dataBlockTableField = new DataBlockTable();
        TagStructTable tagStructTable = new TagStructTable();
        DataReferenceTable dataReferenceTable = new DataReferenceTable();
        TagReferenceFixUpTable tagReferenceFixUpTable = new TagReferenceFixUpTable();
        ZoneSet zoneSet = new ZoneSet();

        public TagHeader TagHeader { get => tagHeader; set => tagHeader = value; }
        public TagStructTable TagStructTable { get => tagStructTable; set => tagStructTable = value; }
        public DataBlockTable DataBlockTableField { get => dataBlockTableField; set => dataBlockTableField = value; }
        public TagReferenceFixUpTable TagReferenceFixUpTable { get => tagReferenceFixUpTable; set => tagReferenceFixUpTable = value; }
        internal DataReferenceTable DataReferenceTable { get => dataReferenceTable; set => dataReferenceTable = value; }
        internal ZoneSet ZoneSet { get => zoneSet; set => zoneSet = value; }

        public void readIn(Stream f)
        {
            f.Position = 0;
            var tables = new List<HeaderTable<HeaderTableEntry>>();


            tagHeader.readStream(f, new PreLoadSections());
            tagDependencyTable.readTable(f, tagHeader);
            dataBlockTableField.readTable(f, tagHeader);
            tagStructTable.Data_block_table = dataBlockTableField;
            tagStructTable.readTable(f, tagHeader);
            dataReferenceTable.TagStructTableField = tagStructTable;
            dataReferenceTable.readTable(f, tagHeader);
            tagReferenceFixUpTable.DataReferenceTableField = dataReferenceTable;
            tagReferenceFixUpTable.TagDependencyTableField = tagDependencyTable;
            tagReferenceFixUpTable.readTable(f, tagHeader);
        }

        public static bool isValid(Stream f)
        {
            f.Position = 0;

            byte[] hash_magic = new byte[4];
            f.Read(hash_magic, 0, 4);
            //("ucsh");
            f.Position = 0;
            return BitConverter.ToInt32(hash_magic) == 1752392565;
        }

        public int tryReadGlobalId(Stream f)
        {
            try
            {
                tagHeader.readStream(f, new PreLoadSections());
                if (tagHeader.Loaded)
                {

                    tagStructTable.Data_block_table = dataBlockTableField;
                    var entry = tagStructTable.readTableItem(f, tagHeader, 0);
                    if (entry!=null)
                        return BitConverter.ToInt32(entry.Bin_datas[0].ToArray(), 8);
                }
            }
            catch (Exception ex)
            {

            }
            return -1;
        }
        byte[] getSection3Bytes()
        {

            return null;
        }
    }
}
