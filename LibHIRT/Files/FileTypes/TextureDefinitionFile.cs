namespace LibHIRT.Files.FileTypes
{

    [FileExtension(".td")]
    public class TextureDefinitionFile : SSpaceFile
    {

        #region Properties

        public override string FileTypeDisplay => "Texture Definition (.td)";

        #endregion

        #region Constructor

        public TextureDefinitionFile(string name, HIRTStream baseStream,
          long dataStartOffset, long dataEndOffset,
          ISSpaceFile parent = null)
          : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
        }

        #endregion

    }

}
