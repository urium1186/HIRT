using LibHIRT.Domain;
using LibHIRT.Serializers;

namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("ocur")]
    [FileSignature("6E73F0380F48411E")]
    [FileExtension(".customizationthemeconfiguration")]
    public class ObjectCustomizationThemeConfiguration : SSpaceFile
    {
        private DinamycType? _deserialized;
        public DinamycType? Deserialized
        {
            get
            {
                if (_deserialized == null)
                    _deserialized = GenericSerializer.Deserialize(GetStream(), this, null);
                return _deserialized;
            }
        }
        public ObjectCustomizationThemeConfiguration(string name, ISSpaceFile parent = null) : base(name, parent)
        {
            TagGroup = "ocur";
        }

        public override string FileTypeDisplay => "ThemeConfiguration *.customizationthemeconfiguration";
    }
}
