namespace LibHIRT.Files
{
    public class FileSignatureAttribute : Attribute
    {

        public string Signature { get; }

        public FileSignatureAttribute(string sign)
        {
            Signature = sign;
        }

        private byte[] GetByteSignature()
          => System.Text.Encoding.UTF8.GetBytes(Signature);

    }
}
