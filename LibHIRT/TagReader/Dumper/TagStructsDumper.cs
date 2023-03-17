
using static LibHIRT.TagReader.Dumper.StructureLayouts;
using System.Xml;
using Memory;
using File = System.IO.File;
using System.Diagnostics;
using System.IO;
using SharpDX;
using System.Runtime.InteropServices;

namespace LibHIRT.TagReader.Dumper
{
    public class TagStructsDumper
    {
        #region Variables
        public XmlWriter textWriter;
        private Mem m = new Mem();
        private XmlWriterSettings xmlWriterSettings = new()
        {
            Indent = true,
            IndentChars = "\t",
        };
        private long startAddress = 0;
        private int tagCount = 0;
        private string outDIR = "";
        private HashSet<int> unique_items_7 = new HashSet<int>();

        private HashSet<string> unique_string_used = new HashSet<string>();

        public Mem M { get => m; set => m = value; }
        public string OutDIR { get => outDIR; set => outDIR = value; }
        public long StartAddress { get => startAddress; set => startAddress = value; }
        public int TagCount { get => tagCount; set => tagCount = value; }

        /*
private HashSet<int> unique_items_1 = new HashSet<int>();
private HashSet<int> unique_items_2 = new HashSet<int>();
private HashSet<int> unique_items_4 = new HashSet<int>();
private HashSet<int> unique_items_5 = new HashSet<int>();
private HashSet<int> unique_items_6 = new HashSet<int>();

private HashSet<int> unique_items_8 = new HashSet<int>();        
private HashSet<int> unique_items_9 = new HashSet<int>();
*/
        #endregion

        #region Other
        public async Task Dump()
        {

            if (outDIR.Length > 1)
            {
                if (m.OpenProcess("HaloInfinite.exe"))
                {
                    await Scan();
                }
            }
            else
            {
                SetStatus("Please select a directory!");
            }

        }

        private async Task Scan()
        {
            SetStatus("Scanning for starting address...");
            if (startAddress == 0 || m.ReadLong((startAddress + 12).ToString("X")) != 7013337615930712659)
                await AoBScan();
            //startAddress = 2178283012096;
            Console.WriteLine(startAddress.ToString());
            if (startAddress != 0)
            {
                SetStatus("Address Found: " + startAddress.ToString("X"));

                int warnings = 0;
                long curAddress = startAddress;
                bool scanning = true;

                while (scanning)
                {
                    if (m.ReadInt((curAddress + 80).ToString("X")) == 257)
                    {
                        tagCount++;
                        curAddress += 88;
                        warnings = 0;
                    }
                    else
                    {
                        warnings++;
                        curAddress += 88;
                    }

                    if (warnings > 3)
                    {
                        scanning = false;
                    }
                }

                SetStatus("Found " + tagCount + " tag structs!");
                DumpStructs();
                printSaveLogUniqueStr();
                SetStatus("Done!");
            }
        }

        private async Task AoBScan()
        {
            long[] results = (await m.AoBScan("?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 53 62 6F 47 67 61 54 61", true, false)).ToArray();
            startAddress = results[0];
        }

        public void SetStatus(string message)
        {

        }
        #endregion

        #region GamerGotten's Slightly Edited Work
        public void DumpStructs()
        {
            try
            {
                List<string> list = new List<string>();
                for (int iteration_index = 0; iteration_index < tagCount; iteration_index++)
                {
                    string temp_filename = outDIR + @"\dump" + iteration_index + ".xml";
                    using (XmlWriter w = XmlWriter.Create(temp_filename, xmlWriterSettings))
                    {
                        textWriter = w;
                        textWriter.WriteStartDocument();
                        textWriter.WriteStartElement("root");

                        long offset_from_start = iteration_index * 88;
                        long current_tag_struct_Address = startAddress + offset_from_start;
                        long gdshgfjasdf = (current_tag_struct_Address);
                        string group_name_thingo = m.ReadString((current_tag_struct_Address + 12).ToString("X"), "", 4);
                        var taggroup = new string(group_name_thingo.Reverse().ToArray());
                        if (taggroup == "ocgd")
                        {
                        }

                        GetGDLS(m.ReadLong((current_tag_struct_Address + 32).ToString("X")));

                        textWriter.WriteEndElement();
                        textWriter.WriteEndDocument();
                        textWriter.Close();

                        System.IO.FileInfo fi = new System.IO.FileInfo(temp_filename);
                        if (fi.Exists)
                        {
                            string s33 = ReverseString(group_name_thingo);
                            if (!s33.Contains("*"))
                            {
                                if (File.Exists(outDIR + @"\" + s33 + ".xml") && list.Contains(s33))
                                {
                                    s33 = s33 + "1";
                                    fi.MoveTo(outDIR + @"\" + s33 + ".xml", true);
                                }
                                else
                                {
                                    fi.MoveTo(outDIR + @"\" + s33 + ".xml", true);
                                }
                            }
                            else
                            {
                                string s331 = s33.Replace("*", "_");
                                fi.MoveTo(outDIR + @"\" + s331 + ".xml", true);
                                //SetStatus(s33 + " replaced with " + s331);
                            }
                            list.Add(s33);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //SetStatus("Failed to dump!");
            }
        }
        public async void addUniqueString(string p_str)
        {
            await addUniqueStringIn(p_str);
            var l = p_str.Split("::");
            if (l.Length > 1)
            {
                foreach (var item in l)
                {
                    await addUniqueStringIn(item);
                }
            }
            var l1 = p_str.Split("_");
            if (l1.Length > 1)
            {
                foreach (var item in l1)
                {
                    await addUniqueStringIn(item);
                }
            }
        }

        public async Task addUniqueStringIn(string p_str)
        {
            lock (unique_string_used)
            {
                unique_string_used.Add(p_str);
            }

        }

        public void printSaveLogUniqueStr()
        {
            return;
            string fullPath = @"H:\GameDev\games-tool\  _!ExtraerModelsHalo\HaloInfinite\HaloInfiniteModelExtractor-main\halo_infinite_tag_reader\hash\asdtag.txt";
            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                foreach (var item in unique_string_used.AsEnumerable())
                {
                    Console.WriteLine("00000000:" + item.ToString());
                    writer.WriteLine("00000000:" + item.ToString());
                }
            }


        }
        public Group_definitions_link_struct GetGDLS(long address,short field_pointer_offet = 24)
        {
            string temp_name_1 = m.ReadString(m.ReadLong(address.ToString("X")).ToString("X"), "", 300);
            string temp_name_2 = m.ReadString(m.ReadLong((address + 8).ToString("X")).ToString("X"), "", 300);

            //addUniqueString(temp_name_1);
            //addUniqueString(temp_name_2);

            Group_definitions_link_struct gdls = new Group_definitions_link_struct
            {
                name1 = temp_name_1,
                name2 = temp_name_2,

                int1 = m.ReadInt((address + 16).ToString("X")),
                int2 = m.ReadInt((address + 20).ToString("X")), // potential count

                Table2_struct_pointer2 = m.ReadLong((address + field_pointer_offet).ToString("X")),
                Table2_struct = ReadChunk(m.ReadLong((address + field_pointer_offet).ToString("X"))), // next

            };

            return gdls;
        }

        public Table2_struct ReadChunk(long address)
        {
            // aasdasd asdad
            byte[] debug = m.ReadBytes(address.ToString("X"), 120);
            string str_bytes = BitConverter.ToString(debug).Replace("-", "");

            bool is37 = false;
            int is37Count = 0;
            long address_for_our_string_bruh = m.ReadLong(address.ToString("X"));
            long address_for_our_string_bruh_1 = m.ReadLong((address + 8).ToString("X"));

            long address_four_our_fields = m.ReadLong((address + 32).ToString("X"));
            int amount_of_things_to_read = m.ReadInt((address + 120).ToString("X"));

            string take_this_mf_and_pass_it_down_for_gods_sake = m.ReadString(address_for_our_string_bruh.ToString("X"), "", 300);
            string take_this_mf_and_pass_it_down_for_gods_sake_1 = m.ReadString(address_for_our_string_bruh_1.ToString("X"), "", 300);
            addUniqueString(take_this_mf_and_pass_it_down_for_gods_sake);
            addUniqueString(take_this_mf_and_pass_it_down_for_gods_sake_1);

            if (textWriter.WriteState == WriteState.Element)
            {
                textWriter.WriteAttributeString("item_name_1", take_this_mf_and_pass_it_down_for_gods_sake);
                textWriter.WriteAttributeString("item_name_2", take_this_mf_and_pass_it_down_for_gods_sake_1);
            }

            for (int index = 0; index < amount_of_things_to_read; index++)
            {
                long address_next_next = address_four_our_fields + (index * 24);

               
                int group = m.ReadInt((address_next_next + 8).ToString("X"));
                string n_name = m.ReadString(m.ReadLong(address_next_next.ToString("X")).ToString("X"), "", 300);

                long next_next_next_address = m.ReadLong((address_next_next + 16).ToString("X"));
                
                #region debug
                int flagSH2 = m.Read2Byte((address_next_next + 4).ToString("X"));
                if (flagSH2 == 0)
                {
                    Debug.Assert(n_name == "");
                    Debug.Assert(group == 0x3B);
                    Debug.Assert(amount_of_things_to_read - 1 == index);
                }
                if (group == 0x3B)
                {
                    Debug.Assert(flagSH2 == 0);
                }
                #endregion


                textWriter.WriteStartElement("_" + group.ToString("X"));
                textWriter.WriteAttributeString("v", n_name);
                addUniqueString(n_name);
                if ("chunkInfo" == n_name)
                {
                }
                switch (group)
                {
                    case 0x2:
                        possible_t1_struct_c_instance ptsct_02 = new possible_t1_struct_c_instance
                        {
                            _02_ = new _02
                            {
                                exe_pointer = m.ReadLong(next_next_next_address.ToString("X"))
                            }
                        };
                        break;
                    case 0xA:
                        TryGetPossibleStructInstance(next_next_next_address);
                        break;
                    case 0xB:
                        TryGetPossibleStructInstance(next_next_next_address);
                        break;
                    case 0xC:
                        TryGetPossibleStructInstance(next_next_next_address);
                        break;
                    case 0xD:
                        TryGetPossibleStructInstance(next_next_next_address);
                        break;
                    case 0xE:
                        TryGetPossibleStructInstance(next_next_next_address);
                        break;
                    case 0xF:
                        TryGetPossibleStructInstance(next_next_next_address);
                        break;
                    case 0x29:
                        new possible_t1_struct_c_instance
                        {
                            _29_ = new _29
                            {
                                //tag_struct_pointer = read_a_Group_definitions_link_struct(address)
                            }
                        };
                        break;
                    case 0x2A:
                        new possible_t1_struct_c_instance
                        {
                            _2A_ = new _2A
                            {
                                //tag_struct_pointer = read_a_Group_definitions_link_struct(address)
                            }
                        };
                        break;
                    case 0x2B:
                        new possible_t1_struct_c_instance
                        {
                            _2B_ = new _2B
                            {
                                //tag_struct_pointer = read_a_Group_definitions_link_struct(address)
                            }
                        };
                        break;
                    case 0x2C:
                        new possible_t1_struct_c_instance
                        {
                            _2C_ = new _2C
                            {
                                //tag_struct_pointer = read_a_Group_definitions_link_struct(address)
                            }
                        };
                        break;
                    case 0x2D:
                        new possible_t1_struct_c_instance
                        {
                            actual_value = next_next_next_address
                        };
                        break;
                    case 0x2E:
                        new possible_t1_struct_c_instance
                        {
                            _2E_ = new _2E
                            {
                                //tag_struct_pointer = read_a_Group_definitions_link_struct(address)
                            }
                        };
                        break;
                    case 0x2F:
                        new possible_t1_struct_c_instance
                        {
                            actual_value = next_next_next_address
                        };
                        break;
                    case 0x30:
                        new possible_t1_struct_c_instance
                        {
                            _30_ = new _30
                            {
                                //tag_struct_pointer = read_a_Group_definitions_link_struct(address)
                            }
                        };
                        break;
                    case 0x31:
                        new possible_t1_struct_c_instance
                        {
                            actual_value = next_next_next_address
                        };
                        break;
                    case 0x34:
                        textWriter.WriteAttributeString("length", next_next_next_address.ToString());
                        new possible_t1_struct_c_instance
                        {
                            actual_value = next_next_next_address
                        };
                        break;
                    case 0x35:
                        textWriter.WriteAttributeString("length", next_next_next_address.ToString());
                        new possible_t1_struct_c_instance
                        {
                            actual_value = next_next_next_address
                        };
                        break;
                    case 0x36:
                        new possible_t1_struct_c_instance
                        {
                            actual_value = next_next_next_address
                        };
                        break;
                    case 0x37:
                        is37 = !is37;
                        is37Count += is37 ? 1 : -1;
                        new possible_t1_struct_c_instance
                        {
                            actual_value = next_next_next_address
                        };
                        break;
                    case 0x38:
                        new possible_t1_struct_c_instance
                        {
                            _38_ = new _38
                            {
                                table2_ref = ReadChunk(next_next_next_address)
                            }
                        };
                        break;
                    case 0x39:
                        string name_temp = m.ReadString(m.ReadLong(next_next_next_address.ToString("X")).ToString("X"), "", 300);
                        
                        new possible_t1_struct_c_instance
                        {
                            _39_ = new _39
                            {
                                Name1 = name_temp,
                                int1 = m.ReadInt((next_next_next_address + 8).ToString("X")),
                                int2 = m.ReadInt((next_next_next_address + 12).ToString("X")),
                                long1 = m.ReadLong((next_next_next_address + 16).ToString("X")),
                                //table2_ref = read_the_big_chunky_one(address) // bruh this in the wrong spot
                            }
                        };

                        // i think we can just ingore that stuff
                        int Repeatamount = m.ReadInt((next_next_next_address + 8).ToString("X"));
                        textWriter.WriteAttributeString("count", Repeatamount.ToString());
                        //textWriter.WriteAttributeString("itemKey", name_temp);
                        Debug.Assert(name_temp == n_name);
                        ReadChunk(m.ReadLong((next_next_next_address + 24).ToString("X")));
                        break;
                    case 0x40:
                        possible_t1_struct_c_instance entry_40 = new possible_t1_struct_c_instance
                        {
                            _40_ = new _40
                            {
                                tag_struct_pointer = GetGDLS(next_next_next_address)
                            }
                        };
                        break;
                    case 0x41:
                        long child_address = m.ReadLong((next_next_next_address + 136).ToString("X"));
                        var t= new possible_t1_struct_c_instance
                        {
                            _41_ = new _41
                            {
                                int1 = m.ReadInt((next_next_next_address + 0).ToString("X")),
                                taggroup1 = m.ReadString((next_next_next_address + 4).ToString("X"), "", 4),
                            }
                        };
                        t._41_.taggroupchilds = new List<string>();
                        int tg_i = 0;
                        textWriter.WriteAttributeString("tagGroup", ReverseString(t._41_.taggroup1));
                        textWriter.WriteAttributeString("int1", t._41_.int1.ToString());
                        
                        break;
                    case 0x42:
                        string name_temp1 = m.ReadString(m.ReadLong(next_next_next_address.ToString("X")).ToString("X"), "", 300);
                        addUniqueString(name_temp1);
                        var tg_42=new possible_t1_struct_c_instance
                        {
                            _42_ = new _42
                            {
                                Name1 = name_temp1,
                                int1 = m.ReadInt((next_next_next_address + 8).ToString("X")),
                                int2 = m.ReadInt((next_next_next_address + 12).ToString("X")),
                                int3 = m.ReadInt((next_next_next_address + 16).ToString("X")),
                                int4 = m.ReadInt((next_next_next_address + 20).ToString("X")),
                                long1 = m.ReadLong((next_next_next_address + 24).ToString("X")),
                                long2 = m.ReadLong((next_next_next_address + 32).ToString("X")),
                                long3 = m.ReadLong((next_next_next_address + 40).ToString("X")),
                                long4 = m.ReadLong((next_next_next_address + 48).ToString("X")),
                            }
                        };
                        
                        string sub_n1 = m.ReadString(m.ReadLong((next_next_next_address + 24).ToString("X")).ToString("X"), "", 300);
                        
                        Debug.Assert((sub_n1 == "") || (sub_n1 == "k_maxFunctionSize" && tg_42._42_.int2 != 0));
                        
                        textWriter.WriteAttributeString("sub_entry", name_temp1);
                        textWriter.WriteAttributeString("int1", tg_42._42_.int1.ToString());
                        textWriter.WriteAttributeString("int2", tg_42._42_.int2.ToString());
                        textWriter.WriteAttributeString("int3", tg_42._42_.int3.ToString());
                        if (sub_n1 == "k_maxFunctionSize") {
                            Debug.Assert(tg_42._42_.int1 == 4);
                            Debug.Assert(tg_42._42_.int2 == 2);
                            Debug.Assert(tg_42._42_.int3 == 872);
                            Debug.Assert(n_name == "data");
                            Debug.Assert(name_temp1 == "");
                            textWriter.WriteAttributeString("type", sub_n1);
                        }
                        break;
                    case 0x43:
                        /*long na = m.ReadLong(next_next_next_address.ToString("X"));
                        int i1 = m.ReadInt((next_next_next_address + 8).ToString("X"));
                        int i2 = m.ReadInt((next_next_next_address + 12).ToString("X"));
                        long l2 = m.ReadLong((next_next_next_address + 16).ToString("X"));
                        byte[] debug_2 = m.ReadBytes(next_next_next_address.ToString("X"), 40);
                        string str_bytes_2 = BitConverter.ToString(debug).Replace("-", "");
                        string name_1 = m.ReadString(na.ToString("X"), "", 300);*/

                        possible_t1_struct_c_instance entry_43 = new possible_t1_struct_c_instance
                        {
                            _43_ = new _43
                            {
                                tag_struct_pointer = GetGDLS(next_next_next_address, 16)
                                /*Name1 = name_1,
                                long1 = 0,
                                //table2_ref = read_the_big_chunky_one(address+16),
                                long2 = 0,//m.ReadLong((next_next_next_address + 24).ToString("X"))*/
                            }
                        };
                        //Table2_struct value_r = ReadChunk(l2);
                        break;
                    default:

                        break;
                }

                //
                textWriter.WriteEndElement();


            }
            if (is37Count != 0)
                Debug.Assert(is37Count == 1);
            return new Table2_struct { };
        }
        private void searchFuctions(long adrress) {

            int i1 = 0;

            while (DebugReadFuctions(adrress, i1)!=-1) {
                i1++;
            }
            int i2 = -1;

            while (DebugReadFuctions(adrress, i2) != -1)
            {
                i2--;
            }
        }
        private int DebugReadFuctions(long next_next_next_address_,int i) {
            
            long next_next_next_address = next_next_next_address_ + i * 56;
            string name_temp1 = m.ReadString(m.ReadLong(next_next_next_address.ToString("X")).ToString("X"), "", 300);
            int int1 = m.ReadInt((next_next_next_address + 8).ToString("X"));
            int int2 = m.ReadInt((next_next_next_address + 12).ToString("X"));
            int int3 = m.ReadInt((next_next_next_address + 16).ToString("X"));
            int int4 = m.ReadInt((next_next_next_address + 20).ToString("X"));
            long long1 = m.ReadLong((next_next_next_address + 24).ToString("X"));
            long long2 = m.ReadLong((next_next_next_address + 32).ToString("X"));
            long long3 = m.ReadLong((next_next_next_address + 40).ToString("X"));
            long long4 = m.ReadLong((next_next_next_address + 48).ToString("X"));
            string sub_n1 = m.ReadString(m.ReadLong((next_next_next_address + 24).ToString("X")).ToString("X"), "", 300);
            return name_temp1 =="" || long3!=0 ? - 1: Math.Abs(i);
        }
        private possible_t1_struct_c_instance TryGetPossibleStructInstance(long address)
        {


            int count_of_children = m.ReadInt((address + 8).ToString("X"));
            long children_address = m.ReadLong((address + 16).ToString("X"));
            List<string> childs = new();

            for (int i = 0; i < count_of_children; i++)
            {
                textWriter.WriteStartElement("Flag");

                long address_WHY_WONT_YOU_WORK = m.ReadLong((address + 16).ToString("X"));

                string reuse_me_uh = m.ReadString(m.ReadLong((address_WHY_WONT_YOU_WORK + (i * 8)).ToString("X")).ToString("X"), "", 300);
                childs.Add(reuse_me_uh);

                textWriter.WriteAttributeString("v", reuse_me_uh);
                addUniqueString(reuse_me_uh);

                textWriter.WriteEndElement();
            }
            string temp_name2 = m.ReadString(m.ReadLong(address.ToString("X")).ToString("X"), "", 300);
            Console.WriteLine("000000:" + temp_name2);
            possible_t1_struct_c_instance ptsct_0A = new possible_t1_struct_c_instance
            {
                _0B_through_0F_ = new _0B_through_0F
                {
                    name = temp_name2,
                    count = count_of_children,
                    children = childs
                }
            };

            return ptsct_0A;
        }

        private string ReverseString(string myStr)
        {
            char[] myArr = myStr.ToCharArray();
            Array.Reverse(myArr);
            return new string(myArr);
        }
        #endregion
    }
}
