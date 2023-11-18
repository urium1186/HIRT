namespace LibHIRT.Files.FileTypes
{
    [FileSignature("51f5b0b22475fcb8")]
    [FileExtension(".string_list_resource")]
    public class StringListResourceFile : SSpaceFile
    {
        public StringListResourceFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "String list resource (.string_list_resource)";
    }
}
