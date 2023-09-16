namespace LibHIRT.Files.FileTypes
{
    public class ResourceFile : SSpaceFile
    {
        public ResourceFile(string name, HIRTStream baseStream, long dataStartOffset, long dataEndOffset, ISSpaceFile parent = null) : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
        }

        public override string FileTypeDisplay => " *.www (Resource)";
    }
}
