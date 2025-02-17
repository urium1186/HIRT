using LibHIRT.Domain;
using LibHIRT.Files.Base;
using LibHIRT.Serializers;
using LibHIRT.TagReader;

namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("char")]
    [FileSignature("733765A390E0D4F9")]
    [FileExtension(".character")]
    public class CharacterDefinitionFile : SSpaceFile, HasModel
    {
        DinamycType? _result;
        public CharacterDefinitionFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
            TagGroup = "char";
        }

        public override string FileTypeDisplay => "";

        public ModelFile GetModel()
        {
            if (_result == null)
            {
                _result = GenericSerializer.Deserialize(GetStream(), this, null);
            }

            var ref_id = (_result["unit"] as TagRef)?.Ref_id_int; //["object"]["model"] HasModel
            var fil_id = (this.Parent as ModuleFile)?.GetFileByGlobalId((int)ref_id);
            if (fil_id == null)
            {
                HasModel file_out = HIFileContext.Instance.GetFile((int)ref_id) as HasModel;
                if (file_out != null)
                {
                    return file_out.GetModel();
                }

            }

            return fil_id == null ? null : fil_id is HasModel ? ((HasModel)fil_id).GetModel() : null;
        }
    }
}
