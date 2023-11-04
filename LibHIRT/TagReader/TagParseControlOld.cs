
using LibHIRT.TagReader.Headers;
using Memory;
using System;
using System.Diagnostics;

using static LibHIRT.TagReader.TagLayouts;

namespace LibHIRT.TagReader
{
    public class TagParseControlOld: ITagParseControl
    {
        public event OnInstanceEventHandler OnInstanceFullLoad;
        public event EventHandler<ITagInstance> OnInstanceLoadEvent;

        struct ResultType
        {
            public ParentTagInstance parent;
            public List<CompoundTagInstance> childs;
        };

        string _filename = "";
        string _tagLayoutTemplate = "";
        Dictionary<int, Template?>? _tagLayout;
        TagFile? _tagFile;
        Stream? _f;
        CompoundTagInstance? _rootTagInst;

        public CompoundTagInstance? RootTagInst { get => _rootTagInst; set => _rootTagInst = value; }
        public TagFile? TagFile { get => _tagFile; set => _tagFile = value; }
        public Stream MemoStream { get; private set; }

        public Template TagTemplate => _tagLayout!=null && _tagLayout.Count!=0?_tagLayout[0]:null;

        public TagParseControlFiltter ParseControlFiltter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public TagParseControlOld(string filename, string tagLayoutTemplate, Dictionary<int, Template?>? tagLayout, Stream? f)
        {
            _filename = filename;
            _tagLayoutTemplate = tagLayoutTemplate;
            _tagLayout = tagLayout;
            _f = f;
        }
        public Dictionary<int, Template?> getSubTaglayoutFrom(string tagLayoutStr, string hash)
        {
            return getSubTaglayoutFrom(TagXmlParse.parse_the_mfing_xmls(tagLayoutStr), hash);
        }
        public Dictionary<int, Template?> getSubTaglayoutFrom(Dictionary<int, Template?>? tagLayout, string hash)
        {
            if (tagLayout == null || string.IsNullOrEmpty(hash))
                return null;
            Dictionary<int, Template?> result = null;
            foreach (var item in tagLayout)
            {
                if (item.Value.E != null && item.Value.E.ContainsKey("hashTR0") && item.Value.E["hashTR0"].ToString() == hash)
                {
                    result = new Dictionary<int, Template?>();
                    result[0] = item.Value;
                    return result;
                }
                if (item.Value.B != null && item.Value.B.Count != 0)
                {
                    result = getSubTaglayoutFrom(item.Value.B, hash);
                    if (result != null)
                        return result;
                }

            }
            return null;
        }

        #region On Mem
        public void readOnMem(long address, Mem M)
        {
            try
            {
                if (_tagLayout == null)
                {
                    _tagLayout = TagXmlParse.parse_the_mfing_xmls(_tagLayoutTemplate);
                }

                //C root_tag = new C { T = TagElemntType.RootTagInstance, N = "Root", B = _tagLayout, xmlPath = ("#document\\root", "#document\\root") };
                Template root_tag = _tagLayout[0];

                _rootTagInst = new RootTagInstance(root_tag, 0, 0);
                MemoStream = null;
                readTagsAndCreateInstancesFromMem(ref _rootTagInst, address, M);

            }
            catch (Exception e)
            {

                throw e;
            }
        }

        private void readTagsAndCreateInstancesFromMem(ref CompoundTagInstance instance_parent, long address, Mem m)
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
                        readTagsAndCreateInstancesFromMem(ref tempTb, tempAddr + i * tbloS, m);
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
            var tagDefinitions = ((C)parent.TagDef).B;
            f.Seek(parcial_address, SeekOrigin.Begin);
            int i = 0;
            long parcial_addresstemp = f.Position;
            foreach (var entry in tagDefinitions.Keys)
            {
                parcial_addresstemp = f.Position;
                var childItem = TagInstanceFactory.Create(tagDefinitions[entry], parcial_address, entry);

                tagInstanceTemp.AddChild(childItem);
                childItem.Parent = parent;
                childItem.Content_entry = parent.Content_entry;
                childItem.ReadIn(new BinaryReader(f), _tagFile?.TagHeader);
                if (f.Position < parcial_addresstemp)
                {
                }
                parcial_addresstemp = f.Position;
                VerifyAndAddTagBlocksInMem(parent, tagBlocks, childItem);
                if (childItem is ArrayFixLen)
                {
                    parcial_addresstemp = f.Position;
                    var tempref = (CompoundTagInstance)childItem;
                    int count = (int)childItem.TagDef.E["count"];
                    for (int k = 0; k < count; k++)
                    {
                        parcial_addresstemp = f.Position;
                        var r2 = readTagDefinitionOnMem(ref tempref, f, parcial_addresstemp);
                        tempref.AddChild(r2.Value.Item1);
                        tagBlocks.AddRange((IEnumerable<CompoundTagInstance>)r2.Value.Item2);
                    }
                }
                else
                if (childItem is ParentTagInstance)
                {
                    parcial_addresstemp = f.Position;
                    CompoundTagInstance temp = (CompoundTagInstance)childItem;
                    var temp_r = readTagDefinitionOnMem(ref temp, f, parcial_addresstemp);
                    tagBlocks.AddRange((IEnumerable<CompoundTagInstance>)temp_r.Value.Item2);
                }
                i++;
            }
            return (tagInstanceTemp, tagBlocks);
        }
        private void VerifyAndAddTagBlocksInMem(CompoundTagInstance parent, List<CompoundTagInstance> tagBlocks, TagInstance childItem)
        {
            switch (((C)childItem.TagDef).T)
            {
                case TagElemntType.TagData:
                    //(childItem as TagData).Data_reference = parent.Content_entry.L_function[ref_it.f];
                    //ref_it.f += 1;
                    OnInstanceLoad(childItem);
                    break;
                /*
                tagInstanceTemp[key].data_reference = instance_parent.content_entry.l_function[ref_it['f']]
                if tagInstanceTemp[key].data_reference.unknown_property != 0:
                    debug = 0

                assert self.full_header.file_header.data_reference_count != 0
                assert len(instance_parent.content_entry.l_function[ref_it['f']].bin_data) == tagInstanceTemp[
                    key].byteLengthCount

                self.hasFunction += 1
                ref_it['f'] += 1 */


                case TagElemntType.TagRef:
                    //(childItem as TagRef).Tag_ref = parent.Content_entry.L_tag_ref[ref_it.r];
                    //(childItem as TagRef).loadPath();
                    //ref_it.r += 1;
                    OnInstanceLoad(childItem);
                    break;
                case TagElemntType.TagStructData:
                    /*
                    if (parent.Content_entry.Childs.Count > ref_it.i)
                    {
                        var temp_entry = parent.Content_entry.Childs[ref_it.i];

                        if (parent.Content_entry.Childs[ref_it.i].TypeIdTg == TagStructType.NoDataStartBlock)
                        {
                            tagBlocks.Add((CompoundTagInstance)childItem);
                            ref_it.i += 1;

                        }
                        else
                        {
                            OnInstanceLoad(childItem);
                        }

                    }*/
                    break;
                case TagElemntType.Tagblock:
                    tagBlocks.Add((CompoundTagInstance)childItem);
                    //ref_it.i += 1;
                    break;
                case TagElemntType.ResourceHandle:
                    tagBlocks.Add((CompoundTagInstance)childItem);
                    //ref_it.i += 1;
                    break;
                default:
                    childItem.Parent = parent;
                    OnInstanceLoad(childItem);
                    break;
            }
        }


        #endregion
        #region On Disk
        public void readFile(Dictionary<int, Template?>? tagLayout, TagFile tagFile)
        {
            try
            {
                if (tagLayout == null)
                {
                    return;
                }

                if (tagFile == null)
                    return;
                _tagLayout = tagLayout;
                _tagFile = tagFile;



                Template root_tag = _tagLayout[0];

                _rootTagInst = new RootTagInstance(root_tag, 0, 0);
                _rootTagInst.Content_entry = _tagFile.TagStructTable.Entries[0];
                _rootTagInst.InstanceParentOffset = 0;
                //readTagsAndCreateInstances();
                readTagsAndCreateInstances(ref _rootTagInst);
            }
            catch (Exception e)
            {
                if (_tagLayoutTemplate == "����")
                {

                }
                else
                    throw e;
            }

        }

        public void readFile()
        {
            try
            {
                if (_tagLayout == null)
                {
                    if (!string.IsNullOrEmpty(_tagLayoutTemplate) && _tagLayoutTemplate != "����")
                    {
                        _tagLayout = TagXmlParse.parse_the_mfing_xmls(_tagLayoutTemplate);
                    }
                }
                if (!TagFile.isValid(_f))
                    return;

                _tagFile = new TagFile();
                _tagFile.readIn(_f);

                if (_tagLayout == null || _tagLayout.Count == 0)
                    return;


                //C root_tag = new C { T = TagElemntType.RootTagInstance, N = "Root", B = _tagLayout, xmlPath = ("#document\\root", "#document\\root") };
                Template root_tag = _tagLayout[0];
                //_rootTagInst = new RootTagInstance(root_tag, _tagFile.TagStructTable.Entries[0].Field_data_block.OffsetPlus,0);
                _rootTagInst = new RootTagInstance(root_tag, 0, 0);
                _rootTagInst.Content_entry = _tagFile.TagStructTable.Entries[0];

                //readTagsAndCreateInstances();
                readTagsAndCreateInstances(ref _rootTagInst);
            }
            catch (Exception e)
            {
                if (_tagLayoutTemplate == "����")
                {

                }
                else
                    throw e;
            }

        }

        protected void readTagsAndCreateInstances()
        {
            _rootTagInst.ReadIn();
        }
        string invertHex(string ex) {
            string inverted = "";
            for (int i = ex.Length-2; i >= 0; i-=2)
            {
                inverted+=(ex.Substring(i, 2));
            }
            return inverted;
        }
        public void readTagsAndCreateInstances(ref CompoundTagInstance instance_parent)
        {
            List<CompoundTagInstance> tagBlocks = new List<CompoundTagInstance>();
            if (instance_parent.Content_entry.TypeIdTg == TagStructType.ExternalFileDescriptor)
            {
            }
            instance_parent.Content_entry.Field_name = instance_parent.TagDef.N;
            if (instance_parent is RootTagInstance) {
                if (instance_parent.TagDef.E != null && instance_parent.TagDef.E.ContainsKey("hashTR0"))
                {
                    Debug.Assert(invertHex(_tagFile.TagHeader.TagFileHeaderInst.TypeHashStr) == instance_parent.TagDef.E["hashTR0"].ToString());
                }
            }
            if (instance_parent.TagDef.E != null && instance_parent.TagDef.E.ContainsKey("hash"))
            {
                Debug.Assert(instance_parent.Content_entry.UID == instance_parent.TagDef.E["hash"].ToString());
                //Debug.Assert(instance_parent.TagDef.E["hash"].ToString().Substring(0,16) == instance_parent.Content_entry.UID1); 
            }
            else
            {
                if (((C)instance_parent.TagDef).T != TagElemntType.RootTagInstance)
                {
                }
            }
            if (instance_parent.Content_entry.Field_data_block == null)
            {
                OnInstanceLoad(instance_parent);
                return;
            }
            int n_items = -1;
            (CompoundTagInstance, List<CompoundTagInstance>)? read_result = null;
            RefItCount refItCount = new RefItCount();
            long instParentOffest = 0;
            foreach (var data in instance_parent.Content_entry.Bin_datas)
            {
                MemoryStream bin_stream = new MemoryStream(data.ToArray<byte>());

                read_result = readTagDefinition(instParentOffest, ref instance_parent, bin_stream, ref refItCount);
                if (instance_parent is ParentTagInstance)
                {
                }
                else
                {
                    instance_parent.AddChild(read_result.Value.Item1);
                }
                tagBlocks.AddRange((IEnumerable<CompoundTagInstance>)read_result.Value.Item2);
                n_items = read_result.Value.Item2.Count;
                instParentOffest += data.Count;
            }

            /*
             assert ref_count['f'] == instance_parent.content_entry.l_function.__len__(), f'{self.filename}'
            assert ref_count['r'] == instance_parent.content_entry.l_tag_ref.__len__(), f'{self.filename}'
            */
            if (read_result == null)
            {
                OnInstanceLoad(instance_parent);
                return;
            }

            if (instance_parent.Content_entry.TypeIdTg == TagStructType.ResourceHandle && read_result.Value.Item1 == null)
            {
                OnInstanceLoad(instance_parent);
                return;
            }
            if (n_items == -1)
            {
                if (instance_parent.Content_entry.Childs.Count != 0)
                {

                }
            }
            else if (n_items == 0)
            {
                if (instance_parent.Content_entry.Childs.Count != 0)
                {
                    /*
                     debug = 'error'
                    print(f"Error entry childs mayor q tag lay in {self.filename}, {instance_parent.tagDef.N}")*/
                    OnInstanceLoad(instance_parent);

                    return;
                }

            }
            else
            {
                if (instance_parent.Content_entry.Childs.Count != 0)
                {
                    /*
                      count = divmod(instance_parent.content_entry.childs.__len__(), n_items)
                if instance_parent.content_entry.type_id != TagStructType.ExternalFileDescriptor:
                    if count[1] != 0:
                        debug = "pposible error"
                    if count[0] != instance_parent.content_entry.bin_datas.__len__():
                        debug = "pposible error"
                else:
                    debug = True
                     
                     */
                }
            }
            Debug.Assert(instance_parent.Content_entry.Childs.Count <= tagBlocks.Count);

            for (int i = 0; i < instance_parent.Content_entry.Childs.Count; i++)
            {
                var entry = instance_parent.Content_entry.Childs[i];
                var tag_child_inst = tagBlocks[i];

                if (tag_child_inst.GetType() == typeof(TagStructData))
                {
                    /*
                     assert entry.type_id_tg == TagStructType.NoDataStartBlock, 'Coinciden en tipo NoDataStartBlock'
                assert len(
                    entry.bin_datas) == 0, f'Error in {self.filename},  {instance_parent.tagDef.N}, {tag_child_inst.tagDef.N}'
            
                     */
                }
                else if (tag_child_inst.GetType() == typeof(ResourceHandle))
                {
                    //assert entry.type_id_tg == TagStructType.ResourceHandle or entry.type_id_tg == TagStructType.ExternalFileDescriptor, f'Coinciden en tipo ResourceHandle in {self.filename},  {instance_parent.tagDef.N}'
                    if (entry.TypeIdTg == TagStructType.ResourceHandle)
                    {

                    }
                }
                else
                {

                }
                tag_child_inst.Content_entry = entry;
                tag_child_inst.Parent = instance_parent;
                readTagsAndCreateInstances(ref tag_child_inst);
            }
            OnInstanceLoad(instance_parent);
        }

        private (CompoundTagInstance, List<CompoundTagInstance>)? readTagDefinition(long instParentOffest, ref CompoundTagInstance parent, MemoryStream f, ref RefItCount ref_it, long parcial_address = 0)
        {
            ParentTagInstance tagInstanceTemp = new ParentTagInstance((C)parent.TagDef, 0, 0);
            if (parent is ParentTagInstance)
            {
                tagInstanceTemp = (ParentTagInstance)parent;
            }

            List<CompoundTagInstance> tagBlocks = new List<CompoundTagInstance>();
            var tagDefinitions = ((C)parent.TagDef).B;
            f.Seek(parcial_address, SeekOrigin.Begin);
            int i = 0;
            long parcial_addresstemp = f.Position;
            foreach (var entry in tagDefinitions.Keys)
            {
                parcial_addresstemp = f.Position;
                var childItem = TagInstanceFactory.Create(tagDefinitions[entry], parcial_address, entry);

                tagInstanceTemp.AddChild(childItem);
                childItem.Parent = parent;
                childItem.Content_entry = parent.Content_entry;
                childItem.InstanceParentOffset = instParentOffest + entry;
                childItem.ReadIn(new BinaryReader(f), _tagFile?.TagHeader);
                if (f.Position < parcial_addresstemp)
                {
                }
                parcial_addresstemp = f.Position;
                VerifyAndAddTagBlocks(parent, ref_it, tagBlocks, childItem);
                if (childItem is ArrayFixLen)
                {
                    parcial_addresstemp = f.Position;
                    var tempref = (CompoundTagInstance)childItem;
                    int count = (int)childItem.TagDef.E["count"];
                    for (int k = 0; k < count; k++)
                    {
                        parcial_addresstemp = f.Position;
                        var r2 = readTagDefinition(childItem.InstanceParentOffset, ref tempref, f, ref ref_it, parcial_addresstemp);
                        tempref.AddChild(r2.Value.Item1);
                        tagBlocks.AddRange((IEnumerable<CompoundTagInstance>)r2.Value.Item2);
                    }
                }
                else
                if (childItem is ParentTagInstance)
                {
                    parcial_addresstemp = f.Position;
                    CompoundTagInstance temp = (CompoundTagInstance)childItem;
                    var temp_r = readTagDefinition(childItem.InstanceParentOffset, ref temp, f, ref ref_it, parcial_addresstemp);
                    tagBlocks.AddRange((IEnumerable<CompoundTagInstance>)temp_r.Value.Item2);
                }
                i++;
            }
            return (tagInstanceTemp, tagBlocks);
        }

        private void VerifyAndAddTagBlocks(CompoundTagInstance parent, RefItCount ref_it, List<CompoundTagInstance> tagBlocks, TagInstance childItem)
        {
            switch (((C)childItem.TagDef).T)
            {
                case TagElemntType.TagData:
                    (childItem as TagData).Data_reference = parent.Content_entry.L_function[ref_it.f];
                    ref_it.f += 1;
                    OnInstanceLoad(childItem);
                    break;
                /*
                tagInstanceTemp[key].data_reference = instance_parent.content_entry.l_function[ref_it['f']]
                if tagInstanceTemp[key].data_reference.unknown_property != 0:
                    debug = 0

                assert self.full_header.file_header.data_reference_count != 0
                assert len(instance_parent.content_entry.l_function[ref_it['f']].bin_data) == tagInstanceTemp[
                    key].byteLengthCount

                self.hasFunction += 1
                ref_it['f'] += 1 */


                case TagElemntType.TagRef:
                    (childItem as TagRef).Tag_ref = parent.Content_entry.L_tag_ref[ref_it.r];
                    //(childItem as TagRef).loadPath();
                    ref_it.r += 1;
                    OnInstanceLoad(childItem);
                    break;
                case TagElemntType.TagStructData:
                    if (parent.Content_entry.Childs.Count > ref_it.i)
                    {
                        var temp_entry = parent.Content_entry.Childs[ref_it.i];
                        /*
                         assert not (temp_entry.type_id_tg == TagStructType.NoDataStartBlock and len(
                        temp_entry.bin_datas) != 0), \
                        f'Error in {self.filename}'
                         */

                        if (parent.Content_entry.Childs[ref_it.i].TypeIdTg == TagStructType.NoDataStartBlock)
                        {
                            tagBlocks.Add((CompoundTagInstance)childItem);
                            ref_it.i += 1;

                        }
                        else
                        {
                            OnInstanceLoad(childItem);
                        }

                    }
                    break;
                case TagElemntType.Tagblock:
                    tagBlocks.Add((CompoundTagInstance)childItem);
                    ref_it.i += 1;
                    break;
                case TagElemntType.ResourceHandle:
                    tagBlocks.Add((CompoundTagInstance)childItem);
                    ref_it.i += 1;
                    break;
                default:
                    childItem.Parent = parent;
                    OnInstanceLoad(childItem);
                    break;
            }
        }
        #endregion
        protected void readTagsAndCreateInstances(ref TagInstance instance_parent)
        {
            List<TagInstance> tagBlocks = new List<TagInstance>();
            instance_parent.Content_entry.FieldName = instance_parent.TagDef.N;

            if (instance_parent.Content_entry.Field_data_block == null)
            {
                OnInstanceLoad(instance_parent);
                return;
            }

            int n_items = -1;
            Dictionary<string, object> read_result = new Dictionary<string, object>();
            read_result["instance_parent"] = null;
            read_result["child_array"] = null;

            RefItCount ref_count = new RefItCount
            {
                i = 0,
                f = 0,
                r = 0,
            };

            int k = 0;
            long parentBlockOffset = 0;
            foreach (var data in instance_parent.Content_entry.Bin_datas)
            {
                MemoryStream bin_stream = new MemoryStream(data.ToArray<byte>());
                //var read_result = null;
                read_result = readTagDefinition_(k, parentBlockOffset, ref instance_parent, ref ref_count, f: bin_stream);
                k++;
                //instance_parent.AddChild(read_result["instance_parent"] as Dictionary<string, TagInstance>);
                tagBlocks.AddRange((IEnumerable<CompoundTagInstance>)read_result["child_array"]);
                n_items = ((IEnumerable<TagInstance>)read_result["child_array"]).Count<TagInstance>();
                parentBlockOffset += data.Count;
            }

            /*
              assert ref_count['f'] == instance_parent.content_entry.l_function.__len__(), f'{self.filename}'
             assert ref_count['r'] == instance_parent.content_entry.l_tag_ref.__len__(), f'{self.filename}'
             */

            if (instance_parent.Content_entry.TypeIdTg == TagStructType.ResourceHandle && read_result["instance_parent"] == null)
            {
                OnInstanceLoad(instance_parent);
                return;
            }
            if (n_items == -1)
            {
                if (instance_parent.Content_entry.Childs.Count != 0)
                {

                }
            }
            else if (n_items == 0)
            {
                if (instance_parent.Content_entry.Childs.Count != 0)
                {
                    /*
                     debug = 'error'
                    print(f"Error entry childs mayor q tag lay in {self.filename}, {instance_parent.tagDef.N}")*/
                    OnInstanceLoad(instance_parent);

                    return;
                }

            }
            else
            {
                if (instance_parent.Content_entry.Childs.Count != 0)
                {
                    /*
                      count = divmod(instance_parent.content_entry.childs.__len__(), n_items)
                if instance_parent.content_entry.type_id != TagStructType.ExternalFileDescriptor:
                    if count[1] != 0:
                        debug = "pposible error"
                    if count[0] != instance_parent.content_entry.bin_datas.__len__():
                        debug = "pposible error"
                else:
                    debug = True
                     
                     */
                }
            }
            for (int i = 0; i < instance_parent.Content_entry.Childs.Count; i++)
            {
                var entry = instance_parent.Content_entry.Childs[i];
                var tag_child_inst = tagBlocks[i];

                if (tag_child_inst.GetType() == typeof(TagStructData))
                {
                    /*
                     assert entry.type_id_tg == TagStructType.NoDataStartBlock, 'Coinciden en tipo NoDataStartBlock'
                assert len(
                    entry.bin_datas) == 0, f'Error in {self.filename},  {instance_parent.tagDef.N}, {tag_child_inst.tagDef.N}'
            
                     */
                }
                else if (tag_child_inst.GetType() == typeof(ResourceHandle))
                {
                    //assert entry.type_id_tg == TagStructType.ResourceHandle or entry.type_id_tg == TagStructType.ExternalFileDescriptor, f'Coinciden en tipo ResourceHandle in {self.filename},  {instance_parent.tagDef.N}'
                    if (entry.TypeIdTg == TagStructType.ResourceHandle)
                    {

                    }
                }
                else
                {

                }
                tag_child_inst.Content_entry = entry;
                tag_child_inst.Parent = instance_parent;
                readTagsAndCreateInstances(ref tag_child_inst);
            }

        }

        private void OnInstanceLoad(TagInstance instance)
        {
            if (OnInstanceLoadEvent != null)
                OnInstanceLoadEvent.Invoke(this, instance);
        }

        protected Dictionary<string, object> readTagDefinition_(int i, long parentBlockOffset, ref TagInstance parent, ref RefItCount? ref_it, long parcial_address = 0, Stream? f = null)
        {
            if (ref_it == null)
            {
                ref_it = new RefItCount();
            }

            Dictionary<string, TagInstance> tagInstanceTemp = new Dictionary<string, TagInstance>();
            List<CompoundTagInstance> tagBlocks = new List<CompoundTagInstance>();
            var tagDefinitions = ((C)parent.TagDef).B;
            if (f == null)
                f = _f;
            f.Seek(parcial_address, SeekOrigin.Begin);
            if (tagDefinitions != null)
            {
                foreach (var entry in tagDefinitions.Keys)
                {
                    string key = tagDefinitions[entry].N;
                    string item_key = tagDefinitions[entry].xmlPath.Item2;
                    string parent_key = parent.TagDef.xmlPath.Item2;
                    string sub_key = item_key.Replace(parent_key, "");
                    Debug.Assert(sub_key != item_key);
                    if (parent.GetType() != typeof(RootTagInstance))
                        key = "[" + i.ToString() + "]" + sub_key;
                    else
                        key = sub_key;
                    if (tagInstanceTemp.ContainsKey(key))
                    {
                        key = key + "_";
                    }

                    tagInstanceTemp[key] = TagInstanceFactory.Create(tagDefinitions[entry], parcial_address, entry);
                    tagInstanceTemp[key].Parent = parent;
                    tagInstanceTemp[key].InstanceParentOffset = parentBlockOffset + entry;
                    tagInstanceTemp[key].ReadIn(new BinaryReader(f), _tagFile.TagHeader);

                    switch ((tagDefinitions[entry] as TagLayouts.C).T)
                    {
                        case TagElemntType.TagData:
                            (tagInstanceTemp[key] as TagData).Data_reference = parent.Content_entry.L_function[ref_it.f];
                            ref_it.f += 1;
                            OnInstanceLoad(tagInstanceTemp[key]);
                            break;
                        /*
                        tagInstanceTemp[key].data_reference = instance_parent.content_entry.l_function[ref_it['f']]
                        if tagInstanceTemp[key].data_reference.unknown_property != 0:
                            debug = 0

                        assert self.full_header.file_header.data_reference_count != 0
                        assert len(instance_parent.content_entry.l_function[ref_it['f']].bin_data) == tagInstanceTemp[
                            key].byteLengthCount

                        self.hasFunction += 1
                        ref_it['f'] += 1 */


                        case TagElemntType.TagRef:
                            (tagInstanceTemp[key] as TagRef).Tag_ref = parent.Content_entry.L_tag_ref[ref_it.r];
                            (tagInstanceTemp[key] as TagRef).loadPath();
                            ref_it.r += 1;
                            OnInstanceLoad(tagInstanceTemp[key]);
                            break;
                        case TagElemntType.TagStructData:
                            if (parent.Content_entry.Childs.Count > ref_it.i)
                            {
                                var temp_entry = parent.Content_entry.Childs[ref_it.i];
                                /*
                                 assert not (temp_entry.type_id_tg == TagStructType.NoDataStartBlock and len(
                                temp_entry.bin_datas) != 0), \
                                f'Error in {self.filename}'
                                 */

                                if (parent.Content_entry.Childs[ref_it.i].TypeIdTg == TagStructType.NoDataStartBlock)
                                {
                                    tagBlocks.Add((CompoundTagInstance)tagInstanceTemp[key]);
                                    ref_it.i += 1;

                                }
                                else
                                {
                                    OnInstanceLoad(tagInstanceTemp[key]);
                                }

                            }
                            break;
                        case TagElemntType.Tagblock:
                            tagBlocks.Add((CompoundTagInstance)tagInstanceTemp[key]);
                            ref_it.i += 1;
                            break;
                        case TagElemntType.ResourceHandle:
                            tagBlocks.Add((CompoundTagInstance)tagInstanceTemp[key]);
                            ref_it.i += 1;
                            break;
                        default:
                            tagInstanceTemp[key].Parent = parent;
                            OnInstanceLoad(tagInstanceTemp[key]);
                            break;
                    }
                }
            }
            var result = new Dictionary<string, object>();
            result["instance_parent"] = tagInstanceTemp;
            result["child_array"] = tagBlocks;
            return result;

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
