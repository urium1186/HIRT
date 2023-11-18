using LibHIRT.Domain;
using LibHIRT.Files.Base;
using LibHIRT.Serializers;
using LibHIRT.TagReader;

namespace LibHIRT.Files.FileTypes
{
    public struct ModelInfoToRM
    {
        public Tagblock Variants;
        public Tagblock RuntimeRegions;

    }

    [FileTagGroup("hlmt")]
    [FileSignature("B8265CE5A8DE5BC8")]
    [FileExtension(".model")]
    public class ModelFile : SSpaceFile, HasRenderModel
    {
        DinamycType? _result;
        public ModelFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
            TagGroup = "hlmt";
        }

        public override string FileTypeDisplay => "Model (.model)";

        public RenderModelFile GetRenderModel()
        {
            if (_result == null)
            {
                _result = GenericSerializer.Deserialize(GetStream(), this, null);
            }

            var ref_id = (_result["render model"] as TagRef)?.Ref_id_int;
            var fil_id = (this.Parent as ModuleFile)?.GetFileByGlobalId((int)ref_id);
            if (fil_id == null)
            {
                RenderModelFile file_out = HIFileContext.Instance.GetFile((int)ref_id) as RenderModelFile;
                return file_out;
            }

            return fil_id == null ? null : (RenderModelFile)fil_id;
        }

        public ModelInfoToRM GetModelVariants()
        {
            if (_result == null)
            {
                _result = GenericSerializer.Deserialize(GetStream(), this, null);
            }


            var variants = _result["variants"] as Tagblock;
            var runtime_regions = _result["runtime regions"] as Tagblock;

            return new ModelInfoToRM { Variants = variants, RuntimeRegions = runtime_regions };

        }
    }
}
