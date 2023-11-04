namespace LibHIRT.Files.FileTypes
{
    [FileSignature("shdv")]
    [FileExtension(".shadervariant")]
    public class ShaderVariantFile : SSpaceFile
    {
        public ShaderVariantFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "Shader Variant (.shadervariant)";
    }
}
