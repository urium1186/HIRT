using LibHIRT.Data;
using LibHIRT.Serializers;

namespace LibHIRT.Files.FileTypes
{
    [FileSignature("mode")]
    [FileExtension(".render_model")]
    public class RenderModelFile : SSpaceFile, HasRenderModel
    {


        #region Properties

        public override string FileTypeDisplay => "Render Model (.render_model)";

        #endregion

        #region Constructor
        public RenderModelFile(string name, HIRTStream baseStream, long dataStartOffset, long dataEndOffset, ISSpaceFile parent = null) : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
            TagGroup = "mode";
        }
        #endregion

        #region Public Methods

        public S3DTemplate Deserialize()
        {
            var stream = GetStream();
            try
            {
                stream.AcquireLock();
                return S3DTemplateSerializer.Deserialize(stream);
            }
            finally { stream.ReleaseLock(); }
        }

        public RenderModelFile GetRenderModel()
        {
            return this;
        }

        #endregion
    }
}
