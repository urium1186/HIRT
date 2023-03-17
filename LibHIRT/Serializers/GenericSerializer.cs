using LibHIRT.Domain;
using LibHIRT.Domain.RenderModel;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Serializers
{
    public class GenericSerializer : SerializerBase<DinamycType>
    {
        private static ISSpaceFile _file;

        

        public static DinamycType Deserialize(Stream stream, ISSpaceFile file)
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
            tagParse = new TagParseControl("", _file.TagGroup, null, reader.BaseStream);
            tagParse.readFile();
            obj.Root = tagParse.RootTagInst;
        }
    }
}
