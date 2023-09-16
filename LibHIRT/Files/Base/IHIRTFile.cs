using LibHIRT.Domain;
using LibHIRT.TagReader;

namespace LibHIRT.Files.Base
{
    public interface IHIRTFile
    {
        DinamycType? Deserialized(EventHandler<ITagInstance> _onDeserialized);
        string Name { get; }
        string TagGroup { get; }
    }
}
