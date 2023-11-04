using LibHIRT.Domain;
using LibHIRT.TagReader;

namespace LibHIRT.Files.Base
{
    public interface IHIRTFile:  IDisposable
    {
        DinamycType? Deserialized(TagParseControlFiltter parseControlFiltter = null, bool forceReload = false, EventHandler<ITagInstance> _onDeserialized = null);
        string Name { get; }
        string DisplayName { get; }
        string Extension { get; }
        string TagGroup { get; }

        long ByteSize { get; }

        int TryGetGlobalId();

        void reset();

    }
}
