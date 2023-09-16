namespace LibHIRT.Files.FileTypes
{
    [FileSignature("shbc")]
    [FileExtension(".shader_bytecode")]
    public class ShaderBytecodeFile : SSpaceFile
    {
        public ShaderBytecodeFile(string name, HIRTStream baseStream, long dataStartOffset, long dataEndOffset, ISSpaceFile parent = null) : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
        }

        public override string FileTypeDisplay => "Shader Bytecode (.shader_bytecode)";
    }
}
