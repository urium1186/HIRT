namespace LibHIRT.Files.FileTypes
{
    [FileSignature("shrs")]
    [FileExtension(".shader_root_signature")]
    internal class ShaderRootSignature : SSpaceFile
    {
        public ShaderRootSignature(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "Shader Root Signature (.shader_root_signature)";
    }
}
