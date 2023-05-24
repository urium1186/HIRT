namespace LibHIRT.TagReader.Headers
{
    public abstract class HeaderTable<T> where T : HeaderTableEntry
    {
        protected List<T> entries = new List<T>();

        public List<T> Entries { get => entries; set => entries = value; }

        abstract public void readTable(Stream f, TagHeader header);
        abstract public T readTableItem(Stream f, TagHeader header, int pos);

        public T GetTableEntry(Stream f, TagHeader header, int pos)
        {
            if (pos >= 0 && pos < entries.Count)
            {
                return entries[pos];
            }
            return readTableItem(f, header, pos);
        }

    }
}
