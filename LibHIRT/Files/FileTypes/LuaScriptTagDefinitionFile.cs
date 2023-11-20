namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("luas")]
    [FileSignature("01DD82945FF61375")]
    [FileExtension(".luas")]
    public class LuaScriptTagDefinitionFile : SSpaceFile
    {
        public LuaScriptTagDefinitionFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "luas (.luas)";
    }
}
