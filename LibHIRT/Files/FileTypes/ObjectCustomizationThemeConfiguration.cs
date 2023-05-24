using LibHIRT.Domain;
using LibHIRT.Serializers;

namespace LibHIRT.Files.FileTypes
{
    [FileSignature("ocur")]
    [FileExtension(".customizationthemeconfiguration")]
    public class ObjectCustomizationThemeConfiguration : SSpaceFile
    {
        private DinamycType? _deserialized;
        public DinamycType? Deserialized
        {
            get
            {
                if (_deserialized == null)
                    _deserialized = GenericSerializer.Deserialize(GetStream(), this);
                return _deserialized;
            }
        }
        public ObjectCustomizationThemeConfiguration(string name, HIRTStream baseStream, long dataStartOffset, long dataEndOffset, ISSpaceFile parent = null) : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
            TagGroup = "ocur";
        }

        public override string FileTypeDisplay => "ThemeConfiguration *.customizationthemeconfiguration";
    }
}
