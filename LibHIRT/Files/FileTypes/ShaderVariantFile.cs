namespace LibHIRT.Files.FileTypes
{
    [FileSignature("shdv")]
    [FileExtension(".shadervariant")]
    public class ShaderVariantFile : SSpaceFile
    {
        public ShaderVariantFile(string name, HIRTStream baseStream, long dataStartOffset, long dataEndOffset, ISSpaceFile parent = null) : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
        }

        public override string FileTypeDisplay => "Shader Variant (.shadervariant)";
    }
}
