using LibHIRT.Domain;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.TagReader;

namespace LibHIRT.Serializers
{
    public class GenericSerializer : SerializerBase<DinamycType>
    {
        private static IHIRTFile _file;

        

        public static DinamycType Deserialize(Stream stream, IHIRTFile file)
        {
            _file = file;
            var reader = new BinaryReader(stream);
            return new GenericSerializer().Deserialize(reader);
        }
        protected override void OnDeserialize(BinaryReader reader, DinamycType obj)
        {
            if (obj == null)
            {
                return;
            }
            tagParse = new TagParseControl(_file.Name, _file.TagGroup, null, reader.BaseStream);
            tagParse.readFile();
            obj.Root = tagParse.RootTagInst;
        }
    }
}
