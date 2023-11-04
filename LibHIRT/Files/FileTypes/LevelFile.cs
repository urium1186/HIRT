namespace LibHIRT.Files.FileTypes
{
    [FileSignature("levl")]
    [FileExtension(".level")]
    public class LevelFile : SSpaceFile
    {
        public LevelFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
            TagGroup = "levl";
        }

        public override string FileTypeDisplay => "Level (.level)";
    }
}
