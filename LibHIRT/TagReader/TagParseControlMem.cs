using LibHIRT.TagReader.Headers;
using Memory;
using System.Reflection.PortableExecutable;
using static LibHIRT.TagReader.TagLayoutsV2;

namespace LibHIRT.TagReader
{
    public class TagParseControlMem : ITagParseControl
    {
        private string _tagLayoutTemplate;
        private Dictionary<int, Template?> _tagLayout;

        public CompoundTagInstance RootTagInst => _rootTagInst;

        public Template TagTemplate => _tagLayout?[0];

        public TagFile TagFile => null;

        public Mem Reader { get; set; }

        public long Address { get; set; }

        private RootTagInstance _rootTagInst;

        public Stream MemoStream { get; set; }
        public TagParseControlFiltter ParseControlFiltter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler<ITagInstance> OnInstanceLoadEvent;

        public void readFile()
        {
            readOnMem(Address, Reader);
        }

        public void Dispose()
        {
            
        }

        public TagParseControlMem(string tagGroup) {
            _tagLayoutTemplate = tagGroup;
        }

        public TagParseControlMem( string tagGroup, Mem? reader)
        {
            _tagLayoutTemplate = tagGroup;
            Reader = reader;
        }

        #region On Mem
        
        public void readOnMem(long address, Mem M)
        {
            try
            {
                if (_tagLayout == null)
                {
                    _tagLayout = TagXmlParseV2.parse_the_mfing_xmls(_tagLayoutTemplate);
                }

                //C root_tag = new C { T = TagElemntType.RootTagInstance, N = "Root", B = _tagLayout, xmlPath = ("#document\\root", "#document\\root") };
                Template root_tag = _tagLayout[0];

                _rootTagInst = new RootTagInstance(root_tag, 0, 0);
                MemoStream = null;
                readTagsAndCreateInstancesFromMem(_rootTagInst, address, M);

            }
            catch (Exception e)
            {

                throw e;
            }
        }

        private void readTagsAndCreateInstancesFromMem(CompoundTagInstance instance_parent, long address, Mem m)
        {
            long temp_size = instance_parent.TagDef.S;
            if (temp_size == 0)
            {
                var last = (instance_parent.TagDef as TagLayouts.C).B.Last();
                temp_size = last.Key + last.Value.S;
            }

            byte[] bytes = m.ReadBytes(address.ToString("X"), temp_size);
            var temp_f = new MemoryStream(bytes);
            if (MemoStream == null)
                MemoStream = new MemoryStream(bytes);
            (CompoundTagInstance, List<CompoundTagInstance>)? read_result = readTagDefinitionOnMem(ref instance_parent, temp_f, 0);
            if (instance_parent is ParentTagInstance)
            {
            }
            else
            {
                instance_parent.AddChild(read_result.Value.Item1);
            }
            foreach (var item in read_result.Value.Item2)
            {
                if (item is Tagblock || item is ResourceHandle)
                {
                    var tempTb = item;
                    long tempAddr = item is Tagblock ? ((Tagblock)item).NewAddress : ((ResourceHandle)item).NewAddress;
                    int count = item is Tagblock ? ((Tagblock)item).ChildrenCount : ((ResourceHandle)item).ChildrenCount;
                    for (int i = 0; i < count; i++)
                    {
                        long tbloS = item.TagDef.S;
                        readTagsAndCreateInstancesFromMem(tempTb, tempAddr + i * tbloS, m);
                    }

                }
            }
        }

        private (CompoundTagInstance, List<CompoundTagInstance>)? readTagDefinitionOnMem(ref CompoundTagInstance parent, MemoryStream f, long parcial_address = 0)
        {
            ParentTagInstance tagInstanceTemp = new ParentTagInstance((C)parent.TagDef, 0, 0);
            if (parent is ParentTagInstance)
            {
                tagInstanceTemp = (ParentTagInstance)parent;
            }

            List<CompoundTagInstance> tagBlocks = new List<CompoundTagInstance>();
            var tagDefinitions = ((P)parent.TagDef).B;
            f.Seek(parcial_address, SeekOrigin.Begin);
            int i = 0;
            long parcial_addresstemp = f.Position;
            foreach (var entry in tagDefinitions.Keys)
            {
                parcial_addresstemp = f.Position;
                var childItem = TagInstanceFactoryV2.Create(tagDefinitions[entry], parcial_address, entry);

                tagInstanceTemp.AddChild(childItem);
                childItem.Parent = parent;
                childItem.ReadIn(new BinaryReader(f), null);
                if (f.Position < parcial_addresstemp)
                {
                }
                parcial_addresstemp = f.Position;
                VerifyAndAddTagBlocksInMem(parent, tagBlocks, childItem);
                if (childItem is ArrayFixLen)
                {
                    parcial_addresstemp = f.Position;
                    var tempref = (CompoundTagInstance)childItem;
                    int count = int.Parse(childItem.TagDef.E["count"].ToString());
                    for (int k = 0; k < count; k++)
                    {
                        parcial_addresstemp = f.Position;
                        var r2 = readTagDefinitionOnMem(ref tempref, f, parcial_addresstemp);
                        tempref.AddChild(r2.Value.Item1);
                        tagBlocks.AddRange(r2.Value.Item2);
                    }
                }
                else
                if (childItem is ParentTagInstance)
                {
                    parcial_addresstemp = f.Position;
                    CompoundTagInstance temp = (CompoundTagInstance)childItem;
                    var temp_r = readTagDefinitionOnMem(ref temp, f, parcial_addresstemp);
                    tagBlocks.AddRange(temp_r.Value.Item2);
                }
                i++;
            }
            return (tagInstanceTemp, tagBlocks);
        }
        private void VerifyAndAddTagBlocksInMem(CompoundTagInstance parent, List<CompoundTagInstance> tagBlocks, TagInstance childItem)
        {
            switch (((C)childItem.TagDef).T)
            {
                case TagElemntTypeV2.Data:
                    //(childItem as TagData).Data_reference = parent.Content_entry.L_function[ref_it.f];
                    //ref_it.f += 1;
                    OnInstanceLoad(childItem);
                    break;
               

                case TagElemntTypeV2.TagReference:
                   
                    OnInstanceLoad(childItem);
                    break;
                case TagElemntTypeV2.Struct:
                   
                    break;
                case TagElemntTypeV2.Block:
                    tagBlocks.Add((CompoundTagInstance)childItem);
                    //ref_it.i += 1;
                    break;
                case TagElemntTypeV2.ResourceHandle:
                    tagBlocks.Add((CompoundTagInstance)childItem);
                    //ref_it.i += 1;
                    break;
                default:
                    childItem.Parent = parent;
                    OnInstanceLoad(childItem);
                    break;
            }
        }

        private void OnInstanceLoad(TagInstance instance)
        {
            if (OnInstanceLoadEvent != null)
                OnInstanceLoadEvent.Invoke(this, instance);
        }

        #endregion
    }
}
