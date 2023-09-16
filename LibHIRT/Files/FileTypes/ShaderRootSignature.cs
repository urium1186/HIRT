namespace LibHIRT.Files.FileTypes
{
    [FileSignature("shrs")]
    [FileExtension(".shader_root_signature")]
    internal class ShaderRootSignature : SSpaceFile
    {
        public ShaderRootSignature(string name, HIRTStream baseStream, long dataStartOffset, long dataEndOffset, ISSpaceFile parent = null) : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
        }

        public override string FileTypeDisplay => "Shader Root Signature (.shader_root_signature)";
    }
}
