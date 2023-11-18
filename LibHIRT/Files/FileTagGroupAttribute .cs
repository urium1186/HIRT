namespace LibHIRT.Files
{
    public class FileTagGroupAttribute : Attribute
    {

        public string TagGroup { get; }

        public FileTagGroupAttribute(string tagGroup)
        {
            TagGroup = tagGroup;
        }

        private byte[] GetByteSignature()
          => System.Text.Encoding.UTF8.GetBytes(TagGroup);

    }
}
