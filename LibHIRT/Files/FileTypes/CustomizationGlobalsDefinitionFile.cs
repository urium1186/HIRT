using LibHIRT.Domain;
using LibHIRT.Files.Base;
using LibHIRT.Serializers;
using LibHIRT.TagReader;
using LibHIRT.Utils;
using System.Diagnostics;

namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("ocgd")]
    [FileSignature("C304D1EE96ECBBAD")]
    [FileExtension(".customizationglobalsdefinition")]
    public class CustomizationGlobalsDefinitionFile : SSpaceFile, HasRenderModel
    {
        DinamycType? _result;
        public CustomizationGlobalsDefinitionFile(string name, ISSpaceFile parent = null) : base(name, parent)
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

            var fil_id = HIFileContext.Instance.GetFileFrom((_result["Themes"] as ListTagInstance)[0]["Model"] as TagRef, (ModuleFile)Parent);
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
