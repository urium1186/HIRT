namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("shdv")]
    [FileSignature("984B2CD930519813")]
    [FileExtension(".shadervariant")]
    public class ShaderVariantFile : SSpaceFile
    {
        public ShaderVariantFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "Shader Variant (.shadervariant)";
    }
}
