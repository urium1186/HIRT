namespace LibHIRT.Files.FileTypes
{
    [FileSignature("shbc")]
    [FileExtension(".shader_bytecode")]
    public class ShaderBytecodeFile : SSpaceFile
    {
        public ShaderBytecodeFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "Shader Bytecode (.shader_bytecode)";
    }
}
