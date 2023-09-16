using LibHIRT.Domain;
using LibHIRT.Serializers;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Files.FileTypes
{
    public struct ModelInfoToRM
    {
        public Tagblock Variants;
        public Tagblock RuntimeRegions;

    }

    [FileSignature("hlmt")]
    [FileExtension(".model")]
    public class ModelFile : SSpaceFile, HasRenderModel
    {
        DinamycType? _result;
        public ModelFile(string name, HIRTStream baseStream, long dataStartOffset, long dataEndOffset, ISSpaceFile parent = null) : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
            TagGroup = "hlmt";
        }

        public override string FileTypeDisplay => "Model (.model)";

        public RenderModelFile GetRenderModel()
        {
            if (_result == null) {
                _result = GenericSerializer.Deserialize(GetStream(), this, null);
            }
            
            var ref_id = (_result["render model"] as TagRef)?.Ref_id_int;
            var fil_id = (this.Parent as ModuleFile)?.GetFileByGlobalId((int)ref_id);
            if (fil_id == null) {
                ISSpaceFile file_out;
                HIFileContext.FilesGlobalIdLockUp.TryGetValue((int)ref_id, out file_out);
                return file_out == null ? null : (RenderModelFile)file_out;
            }

            return fil_id==null? null: (RenderModelFile)fil_id;
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
