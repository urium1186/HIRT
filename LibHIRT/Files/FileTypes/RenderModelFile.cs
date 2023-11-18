using LibHIRT.Data;
using LibHIRT.Domain.RenderModel;
using LibHIRT.Files.Base;
using LibHIRT.Serializers;

namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("mode")]
    [FileSignature("D77DD888DF52505D")]
    [FileExtension(".render_model")]
    public class RenderModelFile : SSpaceFile, HasRenderModel
    {
        RenderModelDefinition modelDefinition;

        #region Properties

        public override string FileTypeDisplay => "Render Model (.render_model)";



        #endregion

        #region Constructor
        public RenderModelFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
            TagGroup = "mode";
        }
        #endregion

        #region Public Methods

        public RenderModelDefinition Deserialize(bool forceReload = false) {
            if (modelDefinition == null)
            {
                modelDefinition = RenderModelSerializer.Deserialize(this);
            }
            else if (forceReload) {
                modelDefinition = RenderModelSerializer.Deserialize(this);
            }
            return modelDefinition;
        }

        /*public modelDefinition Deserialize()
        {
            var stream = GetStream();
            try
            {
                stream.AcquireLock();
                var tagParse = base.Deserialized().TagParse;
                return S3DTemplateSerializer.Deserialize(tagParse, stream);
            }
            finally { stream.ReleaseLock(); }
        }*/

        public RenderModelFile GetRenderModel()
        {
            return this;
        }

        #endregion
    }
}
