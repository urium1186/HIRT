using System.Text;

namespace LibHIRT.TagReader.Headers
{
    public abstract class HeaderTableEntry : BinaryReader
    {
        string fieldName = "";
        public int Index { get; set; }
        public int IndexOnParent { get; set; }
        protected HeaderTableEntry(Stream input) : base(input)
        {
        }

        protected HeaderTableEntry(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        protected HeaderTableEntry(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public abstract void ReadIn();
        public abstract int GetSize { get; }
        public string FieldName { get => fieldName; set => fieldName = value; }
    }
}
