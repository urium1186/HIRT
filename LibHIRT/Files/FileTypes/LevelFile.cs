namespace LibHIRT.Files.FileTypes
{
    [FileSignature("levl")]
    [FileExtension(".level")]
    public class LevelFile : SSpaceFile
    {
        public LevelFile(string name, HIRTStream baseStream, long dataStartOffset, long dataEndOffset, ISSpaceFile parent = null) : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
            TagGroup = "levl";
        }

        public override string FileTypeDisplay => "Level (.level)";
    }
}
