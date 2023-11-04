namespace LibHIRT.Files.FileTypes
{
    [FileSignature("sbsp")]
    [FileExtension(".scenario_structure_bsp")]
    public class ScenarioStructureBspFile : SSpaceFile
    {
        public ScenarioStructureBspFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "Scenario Structure Bsp (.sbsp)";
    }
}
