namespace LibHIRT.Files.FileTypes
{
    [FileSignature("sbsp")]
    [FileExtension(".scenario_structure_bsp")]
    public class ScenarioStructureBspFile : SSpaceFile
    {
        public ScenarioStructureBspFile(string name, HIRTStream baseStream, long dataStartOffset, long dataEndOffset, ISSpaceFile parent = null) : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
        }

        public override string FileTypeDisplay => "Scenario Structure Bsp (.sbsp)";
    }
}
