namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("levl")]
    [FileSignature("9B41CED4E0420B4F")]
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
