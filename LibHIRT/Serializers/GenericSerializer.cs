using LibHIRT.Domain;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.TagReader;

namespace LibHIRT.Serializers
{
    public class GenericSerializer : SerializerBase<DinamycType>
    {
        private IHIRTFile _file;
        private TagParseControlFiltter _parseControlFiltter;

        private GenericSerializer(IHIRTFile file, TagParseControlFiltter parseControlFiltter, EventHandler<ITagInstance> evenParameter)
        {
            _file = file;
            _parseControlFiltter = parseControlFiltter;
            OnInstanceLoadEvent += evenParameter;
        }

        public event EventHandler<ITagInstance> OnInstanceLoadEvent;

        public static DinamycType Deserialize(Stream stream, IHIRTFile file,  EventHandler<ITagInstance> evenParameter, TagParseControlFiltter parseControlFiltter=null)
        {
            var reader = new BinaryReader(stream);
            
            return new GenericSerializer(file,parseControlFiltter, evenParameter).Deserialize(reader);
        }
        protected override void OnDeserialize(BinaryReader reader, DinamycType obj)
        {
            if (obj == null)
            {
                return;
            }
            ITagParseControl tagParse = null;
            
            if (_file.TagGroup == "����" || _file.TagGroup is null)
            {
                tagParse = new TagParserControlV2((_file as SSpaceFile).GroupRefHash.Item1, (_file as SSpaceFile).GroupRefHash.Item2, reader.BaseStream);
            }
            else {
                    
                    
                tagParse =  new TagParserControlV2(_file.TagGroup, reader.BaseStream);
                tagParse.ParseControlFiltter = _parseControlFiltter;
                    
            }
            tagParse.OnInstanceLoadEvent += OnInstanceLoadEvent;
            tagParse.readFile();
            tagParse.OnInstanceLoadEvent -= OnInstanceLoadEvent;
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
