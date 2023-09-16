using LibHIRT.Domain;
using LibHIRT.Serializers;
using LibHIRT.TagReader;
using LibHIRT.Utils;
using System.Diagnostics;

namespace LibHIRT.Files.FileTypes
{
    [FileSignature("ocgd")]
    [FileExtension(".customizationglobalsdefinition")]
    public class CustomizationGlobalsDefinitionFile : SSpaceFile, HasRenderModel
    {
        DinamycType? _result;
        public CustomizationGlobalsDefinitionFile(string name, HIRTStream baseStream, long dataStartOffset, long dataEndOffset, ISSpaceFile parent = null) : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
            TagGroup = "ocgd";
        }

        public override string FileTypeDisplay => "GlobalConfiguration *.customizationglobalsdefinition";

        public RenderModelFile GetRenderModel()
        {
            if (_result == null)
            {
                _result = GenericSerializer.Deserialize(GetStream(), this, null);
            }

            var fil_id = HIFileContext.GetFileFrom((_result["Themes"] as ListTagInstance)[0]["Model"] as TagRef, (ModuleFile)Parent);
            if (fil_id == null)
            {
                Debug.Assert(DebugConfig.NoCheckFails, "Check the Mem Files");
            }

            return fil_id == null ? null : (RenderModelFile)fil_id;
        }

        public ListTagInstance GetThemeConfigurations()
        {
            if (_result == null)
            {
                _result = GenericSerializer.Deserialize(GetStream(), this, null);
            }

            return (ListTagInstance)(_result["Themes"] as ListTagInstance)[0]["Theme Configurations"];
        }

    }
}
