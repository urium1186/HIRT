namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("shbc")]
    [FileSignature("C936E02F28F36B6F")]
    [FileExtension(".shader_bytecode")]
    public class ShaderBytecodeFile : SSpaceFile
    {
        public ShaderBytecodeFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "Shader Bytecode (.shader_bytecode)";
    }
}
