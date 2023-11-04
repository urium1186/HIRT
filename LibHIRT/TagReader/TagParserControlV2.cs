using Aspose.ThreeD;
using LibHIRT.TagReader.Headers;
using System.Diagnostics;
using static LibHIRT.TagReader.TagLayoutsV2;

namespace LibHIRT.TagReader
{
    public class TagFileMap
    {
        Dictionary<int, CompoundTagInstance?> blocks = new Dictionary<int, CompoundTagInstance?>();
        Dictionary<int, TagData> datas = new Dictionary<int, TagData?>();
        Dictionary<int, TagRef?> refers = new Dictionary<int, TagRef?>();

        public Dictionary<int, CompoundTagInstance?> Blocks { get => blocks; set => blocks = value; }
        public Dictionary<int, TagData> Datas { get => datas; set => datas = value; }
        public Dictionary<int, TagRef?> Refers { get => refers; set => refers = value; }
    }
    public class TagParserControlV2 : ITagParseControl
    {
        Dictionary<int, Template>? _tagLayout;
        TagFile _tagFile;
        Stream? _f;
        CompoundTagInstance _rootTagInst;

        TagParseControlFiltter parseControlFiltter;

        Dictionary<int, TagFileMap> tag_structs_list = new Dictionary<int, TagFileMap>();

        public event EventHandler<ITagInstance> OnInstanceLoadEvent;

        public Template TagTemplate => _tagLayout != null && _tagLayout.Count != 0 ? _tagLayout[0] : null;

        public TagParserControlV2(Dictionary<int, Template?> tagLayout, Stream? f)
        {
            _tagLayout = tagLayout;
            _tagFile = new TagFile();
            _f = f;
        }

        public TagParserControlV2(string tagGroup, Stream? f)
        {
            _tagLayout = TagXmlParseV2.parse_the_mfing_xmls(tagGroup);
            _tagFile = new TagFile();
            _f = f;
        }

        public TagParserControlV2(string tagGroup, string hash, Stream? f)
        {
            _tagLayout = TagCommon.getSubTaglayoutFrom(tagGroup, hash);
            _tagFile = new TagFile();
            _f = f;
        }

        public CompoundTagInstance? RootTagInst { get => _rootTagInst; set => _rootTagInst = value; }
        public TagFile? TagFile { get => _tagFile; set => _tagFile = value; }
        public TagParseControlFiltter ParseControlFiltter { get => parseControlFiltter; set => parseControlFiltter = value; }

        public void readFile()
        {

            _tagFile.TagStructTable.OnReadEntryEvent += TagStructTable_OnReadEntryEvent;
            _tagFile.DataReferenceTable.OnReadEntryEvent += DataReferenceTable_OnReadEntryEvent;
            _tagFile.TagReferenceFixUpTable.OnReadEntryEvent += TagReferenceFixUpTable_OnReadEntryEvent;
            try
            {
                _tagFile.readIn(_f);
            }
            catch (Exception ex)
            {

                throw ex;
            }

            _tagFile.TagStructTable.OnReadEntryEvent -= TagStructTable_OnReadEntryEvent;
            _tagFile.DataReferenceTable.OnReadEntryEvent -= DataReferenceTable_OnReadEntryEvent;
            _tagFile.TagReferenceFixUpTable.OnReadEntryEvent -= TagReferenceFixUpTable_OnReadEntryEvent;
        }

        private void TagReferenceFixUpTable_OnReadEntryEvent(object? sender, TagReferenceFixup e)
        {

        }

        private void DataReferenceTable_OnReadEntryEvent(object? sender, DataReference e)
        {
            if (!tag_structs_list.ContainsKey(e.Parent_struct_index) || !tag_structs_list[e.Parent_struct_index].Datas.ContainsKey(e.Field_offset))
                return;
            TagData tagData = tag_structs_list[e.Parent_struct_index].Datas[e.Field_offset];
            tagData.Data_reference = e;
        }

        private void TagStructTable_OnReadEntryEvent(object? sender, TagStruct entry)
        {
            
            if (entry == null)
                return;
            TagInstance tag = null;
            var pos_toRetunr = _f.Position;
            bool isLast = false;
            if (entry.TypeIdTg == TagStructType.Root)
            {
                _rootTagInst = (CompoundTagInstance)TagInstanceFactoryV2.Create(_tagLayout[0], 0, 0);
                _rootTagInst.Entry = entry;
                tag = _rootTagInst;
                tag.ReadIn(new BinaryReader(_f));

            }
            else
            {
                // !tag_structs_list[entry.Parent_entry_index].Blocks.ContainsKey(entry.Field_offset) && 
                if (entry.TypeIdTg == TagStructType.NoDataStartBlock)
                    return;
                if (!tag_structs_list.ContainsKey(entry.Parent_entry_index) || !tag_structs_list[entry.Parent_entry_index].Blocks.ContainsKey(entry.Field_offset))
                    return;
                tag = tag_structs_list[entry.Parent_entry_index].Blocks[entry.Field_offset];
                isLast = tag_structs_list[entry.Parent_entry_index].Blocks.Keys.Last() == entry.Field_offset;
                if ((tag.TagDef as TagLayoutsV2.C).T == TagElemntTypeV2.Struct)
                {
                    if (entry.TypeIdTg != TagStructType.NoDataStartBlock)
                        throw new Exception("Error al hacer parse");
                }
                Debug.Assert(tag.TagDef.E["hash"].ToString().ToUpper() == entry.UID.ToUpper());
            }

            TagFileMap outresult = new TagFileMap();

            if (entry.Info.n_childs != -1)
            {
                TagElemntTypeV2? type = (tag.TagDef as TagLayoutsV2.C).T;
                if (type == TagElemntTypeV2.RootTagInstance)
                {
                    readTagDefinition(_f, entry, (P)tag.TagDef, (CompoundTagInstance)tag, outresult, 0);
                }
                else
                {
                    int size = int.Parse(tag.TagDef.E["size"].ToString());
                    for (int i = 0; i < entry.Info.n_childs; i++)
                    {
                        ListItemTagInstance sub_child_elemnt = new ListItemTagInstance(tag.TagDef, 0, 0);
                        sub_child_elemnt.Index = i;
                        sub_child_elemnt.Parent = tag;
                        sub_child_elemnt.ReadIn(new BinaryReader(_f));
                        readTagDefinition(_f, entry, (P)tag.TagDef, sub_child_elemnt, outresult, (i * size));
                        ((ListTagInstance)tag).AddChild(sub_child_elemnt);
                    }
                }
            }
            tag_structs_list[entry.Index] = outresult;
            
                
            _f.Seek(pos_toRetunr, SeekOrigin.Begin);
            if (outresult.Blocks.Count == 0) {
                OnInstanceLoad(tag);
                if (isLast && tag.Parent!=null )
                    OnInstanceLoad(tag.Parent);
            }
                
        }

        private int readTagDefinition(Stream f, TagStruct entry, P tags, CompoundTagInstance parent, TagFileMap outresult, int field_offset = 0)
        {
            foreach (int address in tags.B.Keys)
            {
                var child_lay_tag = tags.B[address];
                if (!applayFilter(child_lay_tag))
                    continue;

                TagInstance child_tag_elemt = TagInstanceFactoryV2.Create(child_lay_tag, field_offset + entry.Field_data_block.OffsetPlus, address);
                child_tag_elemt.Parent = parent;
                //parent[child_lay_tag.N] = child_tag_elemt;
                parent.AddChild(child_tag_elemt);
                child_tag_elemt.ReadIn(new BinaryReader(f));
                verifyAndAddTagBlocks(outresult, child_tag_elemt, field_offset + address);
                if ((child_lay_tag as C).T == TagElemntTypeV2.Struct)
                {
                    readTagDefinition(f, entry, (P)child_lay_tag, (CompoundTagInstance)child_tag_elemt, outresult, field_offset + address);
                    OnInstanceLoad(child_tag_elemt);
                }
                else if ((child_lay_tag as C).T == TagElemntTypeV2.Array)
                {
                    int length = int.Parse(child_lay_tag.E["count"].ToString());
                    int size = int.Parse(child_lay_tag.E["size"].ToString());
                    for (int i = 0; i < length; i++)
                    {
                        ListItemTagInstance sub_child_elemnt = new ListItemTagInstance(child_lay_tag, field_offset + entry.Field_data_block.OffsetPlus, address + (i * size));
                        sub_child_elemnt.Index = i;
                        sub_child_elemnt.Parent = child_tag_elemt;
                        sub_child_elemnt.ReadIn(new BinaryReader(f));
                        readTagDefinition(f, entry, (P)child_lay_tag, sub_child_elemnt, outresult, field_offset + address + (i * size));
                        ((ArrayFixLen)child_tag_elemt).AddChild(sub_child_elemnt);
                    }

                }
            }
            return 0;
        }

        private void OnInstanceLoad(TagInstance instance)
        {
            if (OnInstanceLoadEvent != null)
                OnInstanceLoadEvent.Invoke(this, instance);
        }

        void verifyAndAddTagBlocks(TagFileMap tag_maps, TagInstance child_item, int field_offset)
        {
            TagElemntTypeV2? tagType = (child_item.TagDef as TagLayoutsV2.C).T;

            if (tagType == TagElemntTypeV2.Data)
                tag_maps.Datas[field_offset] = (TagData)child_item;
            else if (tagType == TagElemntTypeV2.TagReference)
                tag_maps.Refers[field_offset] = (TagRef)child_item;
            /*else if (tagType == TagElemntTypeV2.Struct) {
                if (child_item.TagDef.E["comp"].ToString() == "1")
                    tag_maps.Blocks[field_offset] = (ParentTagInstance)child_item;
            } */
            else if (tagType == TagElemntTypeV2.Block || tagType == TagElemntTypeV2.ResourceHandle) {
                if ((child_item as ListTagInstance).ChildrenCount > 0)
                    tag_maps.Blocks[field_offset] = (CompoundTagInstance?)child_item;
            }
            
            else
                return;
        }

        bool applayFilter(Template tag)
        {
            if ((tag as C).T == TagElemntTypeV2.EndStruct)
                return false;
            if (parseControlFiltter != null) {
                foreach (var item_xml_path in parseControlFiltter.PermitedXmlPaths)
                {
                    if (tag.xmlPath.Item2.IndexOf(item_xml_path) == 0)
                        return true;
                }
            }
                
            P child_lay_tag = tag as P;
            if (child_lay_tag == null)
                return true;
            if (child_lay_tag.T == TagElemntTypeV2.EndStruct)
                return false;
            if (parseControlFiltter != null)
            {
                for (int i = 0; i < parseControlFiltter.ClassFilter.Count; i++)
                {
                    ClassFilter item = parseControlFiltter.ClassFilter[i];

                    if (item.Hash != null)
                    {
                        if (child_lay_tag.E["hash"].ToString() == item.Hash)
                        {
                            if (item.Full)
                            {
                                parseControlFiltter.PermitedXmlPaths.Add(child_lay_tag.xmlPath.Item2);

                            }
                            return true;
                        }
                    }
                }

            }else
            {
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            _f.Dispose();
            _tagFile.Dispose();
        }
    }
}
