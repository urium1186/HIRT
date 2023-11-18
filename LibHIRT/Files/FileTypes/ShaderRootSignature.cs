namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("shrs")]
    [FileSignature("C4E813ADE91BD793")]
    [FileExtension(".shader_root_signature")]
    internal class ShaderRootSignature : SSpaceFile
    {
        public ShaderRootSignature(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "Shader Root Signature (.shader_root_signature)";
    }
}
