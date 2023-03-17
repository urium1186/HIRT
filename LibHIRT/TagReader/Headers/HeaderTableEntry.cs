using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader.Headers
{
    public abstract class HeaderTableEntry: BinaryReader
    {
        string fieldName = "";
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
        public abstract int GetSize{ get; }
        public string FieldName { get => fieldName; set => fieldName = value; }
    }
}
