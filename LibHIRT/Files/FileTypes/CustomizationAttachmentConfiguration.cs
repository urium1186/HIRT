using LibHIRT.Domain;
using LibHIRT.Serializers;

namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("ocad")]
    [FileSignature("5BE05C617FB816DF")]
    [FileExtension(".customizationattachmentconfiguration")]
    public class CustomizationAttachmentConfiguration : SSpaceFile
    {
        private DinamycType? _deserialized;

        public CustomizationAttachmentConfiguration(string name, ISSpaceFile parent = null) : base(name, parent)
        {
            TagGroup = "ocad";
        }


        public DinamycType? Deserialized
        {
            get
            {
                if (_deserialized == null)
                    _deserialized = GenericSerializer.Deserialize(GetStream(), this, null);
                return _deserialized;
            }
        }

        public override string FileTypeDisplay => "Customization .customizationattachmentconfiguration";
    }
}
