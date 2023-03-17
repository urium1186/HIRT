using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Files
{
    public class FileSignatureAttribute : Attribute
    {

        public string Signature { get; }

        public FileSignatureAttribute(string signature)
        {
            Signature = signature;
        }

        private byte[] GetByteSignature()
          => System.Text.Encoding.UTF8.GetBytes(Signature);

    }
}
