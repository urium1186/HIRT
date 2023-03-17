using LibHIRT.Data.Textures;
using LibHIRT.Serializers;

namespace LibHIRT.Files.FileTypes
{
    [FileSignature("bitm")]
    [FileExtension(".bitmap")]
    public class PictureFile : SSpaceFile
    {

        #region Properties

        public override string FileTypeDisplay => "Texture (.bitmap)";

        #endregion

        #region Constructor

        public PictureFile(string name, HIRTStream baseStream,
          long dataStartOffset, long dataEndOffset,
          ISSpaceFile parent = null)
          : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
            TagGroup = "bitm";
        }

        #endregion

        #region Public Methods

        public S3DPicture Deserialize()
        {
            var stream = GetStream();
            try
            {
                stream.AcquireLock();
                return S3DPictureSerializer.Deserialize(stream, this);
            }
            finally { stream.ReleaseLock(); }
        }

        #endregion

    }

}
