namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("sbsp")]
    [FileSignature("C2CBAF2EC1957709")]
    [FileExtension(".scenario_structure_bsp")]
    public class ScenarioStructureBspFile : SSpaceFile
    {
        public ScenarioStructureBspFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "Scenario Structure Bsp (.sbsp)";
    }
}
