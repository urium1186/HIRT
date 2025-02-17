using LibHIRT.Domain;
using LibHIRT.Files.Base;
using LibHIRT.Serializers;
using LibHIRT.TagReader;

namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("weap")]
    [FileSignature("E40567E27AB6D442")]
    [FileExtension(".weapon")]
    public class WeaponDefinitionFile : SSpaceFile, HasModel
    {
        DinamycType? _result;
        public WeaponDefinitionFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
            TagGroup = "weap";
        }

        public override string FileTypeDisplay => "";

        public ModelFile GetModel()
        {
            if (_result == null)
            {
                _result = GenericSerializer.Deserialize(GetStream(), this, null);
            }

            var ref_id = (_result["item"]["object"]["model"] as TagRef)?.Ref_id_int;
            var fil_id = (this.Parent as ModuleFile)?.GetFileByGlobalId((int)ref_id);
            if (fil_id == null)
            {
                ModelFile file_out = HIFileContext.Instance.GetFile((int)ref_id) as ModelFile;
                return file_out;
            }

            return fil_id == null ? null : fil_id is ModelFile ? (ModelFile)fil_id : null;
        }
    }
}
