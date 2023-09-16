using LibHIRT.Domain;
using LibHIRT.Serializers;

namespace LibHIRT.Files.FileTypes
{
    [FileSignature("ocad")]
    [FileExtension(".customizationattachmentconfiguration")]
    public class CustomizationAttachmentConfiguration : SSpaceFile
    {
        private DinamycType? _deserialized;

        public CustomizationAttachmentConfiguration(string name, HIRTStream baseStream, long dataStartOffset, long dataEndOffset, ISSpaceFile parent = null) : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
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
