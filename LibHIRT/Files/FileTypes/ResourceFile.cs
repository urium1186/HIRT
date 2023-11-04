namespace LibHIRT.Files.FileTypes
{
    public class ResourceFile : SSpaceFile
    {
        public ResourceFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => " *.www (Resource)";
    }
}
