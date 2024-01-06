
using Memory;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml;
using static LibHIRT.TagReader.Dumper.StructureLayouts;
using File = System.IO.File;

namespace LibHIRT.TagReader.Dumper
{
    [Flags]
    public enum ThreadAccess : int
    {
        TERMINATE = (0x0001),
        SUSPEND_RESUME = (0x0002),
        GET_CONTEXT = (0x0008),
        SET_CONTEXT = (0x0010),
        SET_INFORMATION = (0x0020),
        QUERY_INFORMATION = (0x0040),
        SET_THREAD_TOKEN = (0x0080),
        IMPERSONATE = (0x0100),
        DIRECT_IMPERSONATION = (0x0200)
    }

    
    public class TagStructsDumper
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);


        #region Variables
        public XmlWriter textWriter;
        private Mem m = new Mem();
        private XmlWriterSettings xmlWriterSettings = new()
        {
            Indent = true,
            IndentChars = "\t",
        };
        private long startAddress = 0;
        private long lastAddress = 0;
        private int tagCount = 0;
        private string outDIR = "";
        private HashSet<int> unique_items_7 = new HashSet<int>();

        private HashSet<string> unique_string_used = new HashSet<string>();
        private Stack<string> _37Stack = new Stack<string>();
        private Dictionary<string, long> tags = new Dictionary<string, long>();
        private long[] startAddressList;

        public Mem M { get => m; set => m = value; }
        public string OutDIR { get => outDIR; set => outDIR = value; }
        public long StartAddress { get => startAddress; set => startAddress = value; }
        public int TagCount { get => tagCount; set => tagCount = value; }
        public string GameLocation { get => _gameLocation; set => _gameLocation = value; }

        List<string> tag_group_rev = new List<string>();
        private string _gameLocation;
        private Process _infiniteProc;

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
            //string assd = BitConverter.ToString(BitConverter.GetBytes("tmlh".ToCharArray()[0])).Replace("-", "");
            if (outDIR.Length > 1)
            {

                if (m.OpenProcess("HaloInfinite.exe"))
                {
                    await Scan();
                    m.CloseProcess();
                }
                else {
                    await forceOpenNewInstance();
                }
            }
            else
            {
                SetStatus("Please select a directory!");
            }

        }

        public async Task forceOpenNewInstance()
        {
             if (_gameLocation != "")
            {
                //If it is set, open the game
                _infiniteProc = System.Diagnostics.Process.Start(_gameLocation);
                //wait 15 seconds before trying to load again

                await Task.Delay(10000);
                //await Scan();
                bool cont = false;
                while (!m.OpenProcess("HaloInfinite.exe") || cont)
                {
                    var p = m.mProc?.Process;
                    if (p != null && p.VirtualMemorySize != null) {

                        if (p.VirtualMemorySize >= 137830400)
                            cont = false;
                            
                        
                    }
                    
                }
                await Scan();
                _infiniteProc = m.mProc?.Process;
                m.CloseProcess();
                _infiniteProc.Kill();

            }

        }

        public async Task SearchInMem(string pattern)
        {
            //string assd = BitConverter.ToString(BitConverter.GetBytes("tmlh".ToCharArray()[0])).Replace("-", "");
            
            if (m.OpenProcess("HaloInfinite.exe"))
            {
                await AoBScan_Search(pattern);
            }
            
        }


        private async Task Scan()
        {
            SetStatus("Scanning for starting address...");

            //await AoBScanTTGS();
            //if (startAddress == 0 || m.ReadLong((startAddress + 12).ToString("X")) != 7013337615930712659)
            //await AoBScan();
            
            await AoBScan_aTags(true);

            /*foreach (System.Diagnostics.ProcessThread itemT in _infiniteProc.Threads)
            {
                //m.SuspendThreadByID(itemT.Id);
            } */
            //await AoBScanTestTags();
            //if (startAddressList.Count() > 0) { 
            //}
            //return;
            //await AoBScanTags();
            //startAddress = 2178283012096;
            Console.WriteLine(startAddress.ToString());
            try
            {
                if (startAddressList.Length != 0)
                {
                    SetStatus("Address Found: " + startAddress.ToString("X"));
                    tags.Clear();
                    foreach (var item in startAddressList)
                    {
                        startAddress = item;
                        SearchTagsCounts();
                        SearchTagsCounts(false);
                    }

                    //tagCount = 475;
                    SetStatus("Found " + tags.Count + " tag structs!");
                    //m.DumpMemory(@"D:\fbx_test\dump_memory\HI_dump.dump");
                    
                    DumpStructs();
                    printSaveLogUniqueStr();
                    SetStatus("Done!");
                }
            }
            catch (Exception ex)
            {

                ;
            }

        }

        private void SearchTagsCounts(bool toFront = true)
        {
            int warnings = 0;
            long curAddress = startAddress;
            bool scanning = true;

            while (scanning)
            {
                //string tempTag = m.ReadString((curAddress + 12).ToString("X"), "", 8);
                string tempTag = m.ReadString((curAddress + 12).ToString("X"), "", 4);
                if (validatedTagGroupName(tempTag))
                {
                    tagCount++;
                    /*if (tags.ContainsKey(tempTag.Substring(0, 4)) && curAddress != tags[tempTag.Substring(0, 4)]) {
                        tags["_1"+tempTag.Substring(0, 4)] = curAddress;
                    }
                    else
                    {*/
                    tags[tempTag.Substring(0, 4)] = curAddress;
                    //}

                    lastAddress = curAddress;
                    if (toFront)
                        curAddress += 88;
                    else
                        curAddress -= 88;
                    warnings = 0;
                }
                else
                {
                    scanning = false;
                }
            }
        }
        /*private long SearchTagsCountsBack()
        {
            int warnings = 0;
            long curAddress = startAddress;
            bool scanning = true;

            while (scanning)
            {
                //string tempT = m.ReadString((curAddress + 12).ToString("X"), "", 8);
                string tempT = m.ReadString((curAddress + 12).ToString("X"), "", 4);
                if (validatedTagGroupName(tempT))
                {
                    tagCount++;
                    tags[tempT.Substring(0, 4)] = curAddress;
                    curAddress -= 88;
                    warnings = 0;
                }
                else
                {
                    scanning = false;
                }
            }
            return curAddress;
        }*/

        private static bool validatedTagGroupName(string tempT)
        {
            return tempT != "" && !LibHIRT.Utils.Utils.HasNonASCIIChars(tempT) && tempT.IndexOf("  ") == -1 && tempT.Length >= 4;
        }

        private void SearchTagsCountsOld()
        {
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
        }

        private async Task AoBScan()//hlmt tmlh 746D6C68
        {

            long[] results = (await m.AoBScan("?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 74 6D 6C 68 67 61 54 61", true, false)).ToArray();
            startAddress = results[0];
        }

        private async Task AoBScanTags()
        {
            long[] results = (await m.AoBScan("?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 53 62 6F 47 67 61 54 61", true, false)).ToArray();
            startAddressList = results;
        }
         private async Task AoBScanTestTags()
        {
            // long[] results = (await m.AoBScan("?? ?? ?? ?? 45 FC 02 28 04 DE F6 40 93 66 35 3B 88 24 D3 3A", true, false)).ToArray();
            // long[] results = (await m.AoBScan("?? ?? ?? ?? ?? ?? ?? ?? 91 E4 91 9A 7C 59 7B 42", true, false)).ToArray();
            long[] results = (await m.AoBScan("?? ?? ?? ?? ?? ?? ?? ?? A3 CC F1 B8 AC F5 8E 06", true, false)).ToArray();
            if (results.Length > 0) {
                var temp = m.ReadBytes(results[0].ToString("X"), 16);
                string ts= m.ReadString(results[0].ToString("X"),"", 4); 
                int int1 = m.ReadInt((results[0] + 4).ToString("X"));
            }
            startAddressList = results;
        }

        private async Task AoBScan_aTags(bool force)
        {
            long[] results = (await m.AoBScan("?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 67 61 54 61 FF FF FF FF", true, false)).ToArray();
            
            startAddressList = results;
            if (startAddressList.Length == 0)
            {
                while (!m.OpenProcess("HaloInfinite.exe"))
                {
                }
                
                await AoBScan_aTags(force);
            }
            _infiniteProc = m.mProc?.Process;
        } 
        
        private async Task AoBScan_Search(string search)
        {
            long[] results = (await m.AoBScan(search, true, false)).ToArray();
            startAddressList = results;
        }

        private async Task AoBScanOld()
        {
            long[] results = (await m.AoBScan("?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 53 62 6F 47 67 61 54 61", true, false)).ToArray();
            startAddress = results[0];
        }
        private async Task AoBScanTTGS()
        {
            long[] results = (await m.AoBScan("?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 32 4C 21 3E 1B 4D 3C 57 A0 0F F5 98 99 B4 37 0E", true, false)).ToArray();

            //long[] results = (await m.AoBScan("75 63 73 68 ?? ?? ?? ?? 32 4C 21 3E 1B 4D 3C 57 A0 0F F5 98 99 B4 37 0E", true, false)).ToArray();
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
                //startAddress -= 35200;
                //for (int iteration_index = 0; iteration_index < tagCount; iteration_index++)
                int iteration_index = 0;
                string str_bytes_tags = "";
                byte[] debug_init = m.ReadBytes((startAddress - (10 * 88)).ToString("X"), 10 * 88);
                string str_bytes_init = BitConverter.ToString(debug_init).Replace("-", "");
                long current_tag_struct_Address = 0;
                //getInfoProcess();
                SuspendProcess(0);
                foreach (var tag in tags)
                {
                    
                    string temp_filename = outDIR + @"\dump" + iteration_index + ".xml";
                    _37Stack.Clear();
                    using (XmlWriter w = XmlWriter.Create(temp_filename, xmlWriterSettings))
                    {
                        textWriter = w;
                        textWriter.WriteStartDocument();
                        textWriter.WriteComment(" saved from url=(0016)http://localhost ");
                        textWriter.WriteStartElement("root");
                        //long offset_from_start = iteration_index * 88;
                        //long current_tag_struct_Address = startAddress + offset_from_start;
                        current_tag_struct_Address = tag.Value;
                        long gdshgfjasdf = (current_tag_struct_Address);
                        //string group_name_thingo = m.ReadString((current_tag_struct_Address + 12).ToString("X"), "", 4);
                        string group_name_thingo = tag.Key;
                        // Debug.Assert(tag.Key == group_name_thingo);
                        addUniqueString(group_name_thingo);
                        var taggroup = new string(group_name_thingo.Reverse().ToArray());
                        Debug.Print("Start read of " + taggroup);
                        addUniqueString(taggroup);
                        if (taggroup == "")
                        {
                            continue;
                        }

                        byte[] debug = m.ReadBytes(current_tag_struct_Address.ToString("X"), 88);
                        string str_bytes = BitConverter.ToString(debug).Replace("-", "");
                        str_bytes_tags += str_bytes;
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
                        Debug.Print("End read of " + taggroup);
                    }
                    iteration_index++;
                    
                }
                ResumeProcess(0);
                byte[] debug_end = m.ReadBytes((lastAddress + (10 * 88)).ToString("X"), 10 * 88);
                string str_bytes_end = BitConverter.ToString(debug_end).Replace("-", "");
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
            string fullPath = @"D:\HaloInfiniteStuft\Extracted\Converted\Tags\shader_str\test\asdtag.txt";
            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                foreach (var item in unique_string_used.AsEnumerable())
                {
                    //Console.WriteLine("00000000:" + item.ToString());
                    //writer.WriteLine("00000000:" + item.ToString());
                    writer.WriteLine(item.ToString());
                }
            }


        }
        public Group_definitions_link_struct GetGDLS(long address, short field_pointer_offet = 24)
        {
            string temp_name_1 = m.ReadString(m.ReadLong(address.ToString("X")).ToString("X"), "", 300);
            string temp_name_2 = m.ReadString(m.ReadLong((address + 8).ToString("X")).ToString("X"), "", 300);

            addUniqueString(temp_name_1);
            addUniqueString(temp_name_2);

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
            byte[] debug = m.ReadBytes(address.ToString("X"), 192);
            string str_bytes = BitConverter.ToString(debug).Replace("-", "");

            bool is37 = false;
            int is37Count = 0;
            _37Stack.Clear();
            long address_for_our_string_bruh = m.ReadLong(address.ToString("X")); // 0 -> 7  = 8
            long address_for_our_string_bruh_1 = m.ReadLong((address + 8).ToString("X")); // 8 -> 15  = 8
            
            string hash = BitConverter.ToString(m.ReadBytes((address+ 16).ToString("X"), 16)).Replace("-", ""); // 16 -> 31  = 16

            long address_four_our_fields = m.ReadLong((address + 32).ToString("X")); // 32 -> 39  = 8

            int size = m.ReadInt((address + 40).ToString("X")); // 40 -> 43  = 4
            int unk_int_2 = m.ReadInt((address + 44).ToString("X")); // 44 -> 47  = 4
            Debug.Assert(unk_int_2 <= 10);
            long addr_unk_0 = m.ReadLong((address + 48).ToString("X")); // 48 ->  55 = 8
            var by = m.ReadBytes(m.ReadLong((addr_unk_0).ToString("X")).ToString("X"), 8);
            if (by != null) {
                string a = BitConverter.ToString(by);
                if (a != "")
                {
                }
            }
            
            //string unk_bytes_0 = BitConverter.ToString(m.ReadBytes((address + 48).ToString("X"), 4)).Replace("-", ""); // 48 -> 51  = 4
            //string unk_bytes_1 = BitConverter.ToString(m.ReadBytes((address + 52).ToString("X"), 4)).Replace("-", ""); // 52 -> 55  = 4

            int unk_int_3 = m.ReadInt((address + 56).ToString("X")); // 56 -> 59  = 4
            int unk_int_4 = m.ReadInt((address + 60).ToString("X")); // 60 -> 63  = 4
            Debug.Assert(unk_int_4 == 0);

            long unk_long_1 = m.ReadLong((address + 64).ToString("X")); // 64 -> 71  = 8
                                                                        //int unk_int_6 = m.ReadInt((address + 68).ToString("X")); // 68 -> 71  = 4
                                                                        //Debug.Assert(unk_int_6 == 0);
            int unk_short_0 = m.Read2Byte((address + 64).ToString("X")); // 96 -> 103  = 8
            int unk_short_1 = m.Read2Byte((address + 66).ToString("X")); // 96 -> 103  = 8
            int unk_short_2 = m.Read2Byte((address + 68).ToString("X")); // 96 -> 103  = 8
            int unk_short_3 = m.Read2Byte((address + 70).ToString("X")); // 96 -> 103  = 8

            string hashTagRelated_0 = BitConverter.ToString(m.ReadBytes((address + 72).ToString("X"), 8)).Replace("-", ""); // 72 -> 79  = 8
            
            string hashTagRelated_1 = BitConverter.ToString(m.ReadBytes((address + 80).ToString("X"), 8)).Replace("-", ""); // 80 -> 87  = 8

            //string unk_bytes_last = BitConverter.ToString(m.ReadBytes((address + 88).ToString("X"), 32)).Replace("-", ""); // 48 -> 51  = 4


            int complex = m.ReadInt((address + 88).ToString("X")); // 88 -> 91  = 4
            int unk_int_6 = m.ReadInt((address + 92).ToString("X")); // 92 -> 95  = 4
            Debug.Assert(complex == 0 || complex == 1);
            Debug.Assert(unk_int_6 == 0 ||  unk_int_6 == 2);
            //Debug.Assert(unk_int_5 == 0);

            long unk_long_2 = m.ReadLong((address + 96).ToString("X")); // 96 -> 103  = 8
            


            int unk_int_7 = m.ReadInt((address + 104).ToString("X")); // 104 -> 107  = 4
            int unk_int_8 = m.ReadInt((address + 108).ToString("X")); // 108 -> 111  = 4
            Debug.Assert(unk_int_4 == 0);

            long unk_long_3 = m.ReadLong((address + 112).ToString("X")); // 112 -> 119  = 8

            int amount_of_things_to_read = m.ReadInt((address + 120).ToString("X")); // 120 -> 123  = 4



            string take_this_mf_and_pass_it_down_for_gods_sake = m.ReadString(address_for_our_string_bruh.ToString("X"), "", 300);
            string take_this_mf_and_pass_it_down_for_gods_sake_1 = m.ReadString(address_for_our_string_bruh_1.ToString("X"), "", 300);

            addUniqueString(take_this_mf_and_pass_it_down_for_gods_sake);
            addUniqueString(take_this_mf_and_pass_it_down_for_gods_sake_1);

            if (take_this_mf_and_pass_it_down_for_gods_sake == "s_render_geometry") { 
            }
            if (textWriter.WriteState == WriteState.Element)
            {
                
                textWriter.WriteAttributeString("T1", take_this_mf_and_pass_it_down_for_gods_sake);
                textWriter.WriteAttributeString("T2", take_this_mf_and_pass_it_down_for_gods_sake_1);
                
                textWriter.WriteAttributeString("hash", hash);
                //textWriter.WriteAttributeString("desirfar-0", str_bytes.Substring(80, 64));
                textWriter.WriteAttributeString("hashTR0", hashTagRelated_0);
                textWriter.WriteAttributeString("hashTR1", hashTagRelated_1);

                textWriter.WriteAttributeString("size", size.ToString()); 
                textWriter.WriteAttributeString("ui2", unk_int_2.ToString());

                textWriter.WriteAttributeString("ui3", unk_int_3.ToString());
                textWriter.WriteAttributeString("ui4", unk_int_4.ToString());

                textWriter.WriteAttributeString("us0", unk_short_0.ToString());
                textWriter.WriteAttributeString("us1", unk_short_1.ToString());
                textWriter.WriteAttributeString("us2", unk_short_2.ToString());
                textWriter.WriteAttributeString("us3", unk_short_3.ToString());
                textWriter.WriteAttributeString("comp", complex.ToString());
                textWriter.WriteAttributeString("ui6", unk_int_6.ToString());
                
                
                textWriter.WriteAttributeString("aottr", amount_of_things_to_read.ToString());
                
                textWriter.WriteAttributeString("au0", addr_unk_0.ToString());
                textWriter.WriteAttributeString("ul1", unk_long_1.ToString());
                textWriter.WriteAttributeString("ul2", unk_long_2.ToString());
                textWriter.WriteAttributeString("ul3", unk_long_3.ToString());


            }
            if (take_this_mf_and_pass_it_down_for_gods_sake == "AnyTag_struct_definition")
            { 
            }
            for (int index = 0; index < amount_of_things_to_read; index++)
            {
                long address_next_next = address_four_our_fields + (index * 24);


                string n_name = m.ReadString(m.ReadLong(address_next_next.ToString("X")).ToString("X"), "", 300); // 0 -> 7  = 8
                addUniqueString(n_name);
                int group = m.ReadInt((address_next_next + 8).ToString("X")); // 8 -> 11  = 4
                string group_ = "_" + group.ToString("X");
                string ts = m.ReadString((address_next_next + 12).ToString("X"), "", 4);
                
                long next_next_next_address = m.ReadLong((address_next_next + 16).ToString("X")); // 16 -> 23  = 8
                //Debug.Assert(size_1 == 0);
                #region debug
                /*int flagSH2 = m.Read2Byte((address_next_next + 4).ToString("X"));
                if (flagSH2 == 0)
                {
                    Debug.Assert(n_name == "");
                    Debug.Assert(group == 0x3B);
                    Debug.Assert(amount_of_things_to_read - 1 == index);
                }
                if (group == 0x3B)
                {
                    Debug.Assert(flagSH2 == 0);
                }*/
                #endregion
                string gr = "_" + group.ToString("X");

                textWriter.WriteStartElement("_" + group.ToString("X"));
                textWriter.WriteAttributeString("v", n_name);
                if (group_ == "_37")
                {
                    Debug.Assert(ts != "");
                    if (!tag_group_rev.Contains(ts))
                        tag_group_rev.Add(ts);
                }
                if (ts != "")
                {
                   // ts = new string(ts.Reverse().ToArray());
                   // textWriter.WriteAttributeString("us", ts);
                    
                }

                try
                {
                    var te = group.ToString("X");

                    if (next_next_next_address > 0 && next_next_next_address < 1000)
                    {
                        textWriter.WriteAttributeString("s", next_next_next_address.ToString());
                        textWriter.WriteAttributeString("sg", "");
                    }
                    else {
                        textWriter.WriteAttributeString("s", TagCommon.GROUP_LENGTHS[(TagElemntTypeV2)group].ToString());
                    }
                    var ss = ((TagElemntTypeV2)group).ToString();
                    textWriter.WriteAttributeString("TT", ((TagElemntTypeV2)group).ToString());

                }
                catch (Exception keyEx)
                {
                    textWriter.WriteAttributeString("s", "-1");

                }
                

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
                        if (next_next_next_address > 0) {
                            textWriter.WriteAttributeString("addr", ptsct_02._02_.exe_pointer.ToString());
                            var byt = m.ReadBytes(ptsct_02._02_.exe_pointer.ToString("X"), 8);
                            if (byt != null) {
                                string s_byt = BitConverter.ToString(byt);
                            }
                                
                        }
                            
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
                        //textWriter.WriteAttributeString("length", next_next_next_address.ToString());
                        new possible_t1_struct_c_instance
                        {
                            actual_value = next_next_next_address
                        };
                        break;
                    case 0x35:
                        //textWriter.WriteAttributeString("length", next_next_next_address.ToString());
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
                        if (is37)
                        {
                            _37Stack.Push(n_name);
                        }
                        else
                        {
                            if (_37Stack.Count != 0)
                            {
                                string s_name = _37Stack.Pop();
                            }
                            else
                            {

                            }

                        }

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
                        addUniqueString(name_temp);
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
                        var t = new possible_t1_struct_c_instance
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
                        var tg_42 = new possible_t1_struct_c_instance
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

                        textWriter.WriteAttributeString("sub_entry", name_temp1);
                        textWriter.WriteAttributeString("int1", tg_42._42_.int1.ToString());
                        textWriter.WriteAttributeString("int2", tg_42._42_.int2.ToString());
                        textWriter.WriteAttributeString("int3", tg_42._42_.int3.ToString());
                        textWriter.WriteAttributeString("int4", tg_42._42_.int4.ToString());
                        if (tg_42._42_.int3 != 0) {
                            var t1= tg_42._42_.int3 / 8;
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
                    case 0x450:
                        /*long na = m.ReadLong(next_next_next_address.ToString("X"));
                        int i1 = m.ReadInt((next_next_next_address + 8).ToString("X"));
                        int i2 = m.ReadInt((next_next_next_address + 12).ToString("X"));
                        long l2 = m.ReadLong((next_next_next_address + 16).ToString("X"));
                        byte[] debug_2 = m.ReadBytes(next_next_next_address.ToString("X"), 40);
                        string str_bytes_2 = BitConverter.ToString(debug).Replace("-", "");
                        string name_1 = m.ReadString(na.ToString("X"), "", 300);*/

                        possible_t1_struct_c_instance entry_45 = new possible_t1_struct_c_instance
                        {
                            _45_ = new _45
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
            if (_37Stack.Count != 0)
            {
                Debug.Assert(_37Stack.Count == 1);
                string last_val = _37Stack.Pop();

            }

            return new Table2_struct { };
        }
        private void searchFuctions(long adrress)
        {

            int i1 = 0;

            while (DebugReadFuctions(adrress, i1) != -1)
            {
                i1++;
            }
            int i2 = -1;

            while (DebugReadFuctions(adrress, i2) != -1)
            {
                i2--;
            }
        }
        private int DebugReadFuctions(long next_next_next_address_, int i)
        {

            long next_next_next_address = next_next_next_address_ + i * 56;
            string name_temp1 = m.ReadString(m.ReadLong(next_next_next_address.ToString("X")).ToString("X"), "", 300);
            addUniqueString(name_temp1);
            int int1 = m.ReadInt((next_next_next_address + 8).ToString("X"));
            int int2 = m.ReadInt((next_next_next_address + 12).ToString("X"));
            int int3 = m.ReadInt((next_next_next_address + 16).ToString("X"));
            int int4 = m.ReadInt((next_next_next_address + 20).ToString("X"));
            long long1 = m.ReadLong((next_next_next_address + 24).ToString("X"));
            long long2 = m.ReadLong((next_next_next_address + 32).ToString("X"));
            long long3 = m.ReadLong((next_next_next_address + 40).ToString("X"));
            long long4 = m.ReadLong((next_next_next_address + 48).ToString("X"));
            string sub_n1 = m.ReadString(m.ReadLong((next_next_next_address + 24).ToString("X")).ToString("X"), "", 300);
            addUniqueString(sub_n1);
            return name_temp1 == "" || long3 != 0 ? -1 : Math.Abs(i);
        }
        private possible_t1_struct_c_instance TryGetPossibleStructInstance(long address)
        {

            int count_of_children = m.ReadInt((address + 8).ToString("X"));
            int unk_int = m.ReadInt((address + 12).ToString("X"));
            long children_address = m.ReadLong((address + 16).ToString("X"));
            List<string> childs = new();
            string temp_name2 = m.ReadString(m.ReadLong(address.ToString("X")).ToString("X"), "", 300);
            if (textWriter.WriteState == WriteState.Element)
            {

                textWriter.WriteAttributeString("T1", temp_name2);
                //textWriter.WriteAttributeString("Funk_int", unk_int.ToString());
            }
            addUniqueString(temp_name2);
            Console.WriteLine("000000:" + temp_name2);
            for (int i = 0; i < count_of_children; i++)
            {
                textWriter.WriteStartElement("Flag");

                long address_WHY_WONT_YOU_WORK = m.ReadLong((address + 16).ToString("X"));

                string reuse_me_uh = m.ReadString(m.ReadLong((address_WHY_WONT_YOU_WORK + (i * 8)).ToString("X")).ToString("X"), "", 300);
                addUniqueString(reuse_me_uh);
                childs.Add(reuse_me_uh);

                textWriter.WriteAttributeString("v", reuse_me_uh);


                textWriter.WriteEndElement();
            }
            
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

        private string GetWmicOutput(string query, bool redirectStandardOutput = true)
        {
            var info = new ProcessStartInfo("wmic");

            info.Arguments = query;
            info.RedirectStandardOutput = redirectStandardOutput;

            var output = string.Empty;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            return output.Trim();
        }

        private void getInfoProcess() {
            //Define 
            string pn = "HaloInfinite.exe";
            var readOpSec = new PerformanceCounter("Process", "IO Read Operations/sec", pn);
            var writeOpSec = new PerformanceCounter("Process", "IO Write Operations/sec", pn);
            var dataOpSec = new PerformanceCounter("Process", "IO Data Operations/sec", pn);
            var readBytesSec = new PerformanceCounter("Process", "IO Read Bytes/sec", pn);
            var writeByteSec = new PerformanceCounter("Process", "IO Write Bytes/sec", pn);
            var dataBytesSec = new PerformanceCounter("Process", "IO Data Bytes/sec", pn);

            var counters = new List<PerformanceCounter>
                {
                readOpSec,
                writeOpSec,
                dataOpSec,
                readBytesSec,
                writeByteSec,
                dataBytesSec
                };

            // get current value
            foreach (PerformanceCounter counter in counters)
            {
                float rawValue = counter.NextValue();

                // display the value
            }
        }

        private void SuspendProcess(int pid)
        {
            var process = _infiniteProc;//Process.GetProcessById(pid); // throws exception if process does not exist

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        public void ResumeProcess(int pid)
        {
            var process = _infiniteProc;// Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
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
