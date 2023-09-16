using LibHIRT.Domain;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.TagReader;
using System.Reflection;

namespace LibHIRT.Serializers
{
    public class GenericSerializer : SerializerBase<DinamycType>
    {
        private static IHIRTFile _file;

        public static event EventHandler<ITagInstance> OnInstanceLoadEvent;

        public static DinamycType Deserialize(Stream stream, IHIRTFile file, EventHandler<ITagInstance> evenParameter)
        {
            _file = file;
            var reader = new BinaryReader(stream);
            OnInstanceLoadEvent += evenParameter;
            return new GenericSerializer().Deserialize(reader);
        }
        protected override void OnDeserialize(BinaryReader reader, DinamycType obj)
        {
            if (obj == null)
            {
                return;
            }
            tagParse = new TagParseControl(_file.Name, _file.TagGroup, null, reader.BaseStream);
            tagParse.OnInstanceLoadEvent += OnInstanceLoad;
            tagParse.readFile();
            obj.Root = tagParse.RootTagInst;
            obj.TagParse = tagParse;
        }

        private void OnInstanceLoad(object? sender, ITagInstance e)
        {
            if (OnInstanceLoadEvent != null)
               OnInstanceLoadEvent.Invoke(sender, e);
        }

    }
}
