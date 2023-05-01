using LibHIRT.Domain;

namespace LibHIRT.Files.Base
{
    public interface IHIRTFile
    {
        DinamycType? Deserialized { get; }
        string Name { get; }
        string TagGroup { get; }
    }
}
