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

        public int BitmapsCount { get; set; }
        public int CurrentBitmapIndex { get; set; }
        public int MaxResMipMapIndex { get; set; }

        #endregion

        #region Constructor

        public PictureFile(string name, HIRTStream baseStream,
          long dataStartOffset, long dataEndOffset,
          ISSpaceFile parent = null)
          : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
            TagGroup = "bitm";
            CurrentBitmapIndex = 0;
            BitmapsCount = 0;
            MaxResMipMapIndex = 0;
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
