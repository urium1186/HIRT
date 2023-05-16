using LibHIRT.TagReader.Common;
using LibHIRT.Utils;
using Memory;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static LibHIRT.TagReader.Headers.TagHeader;

namespace LibHIRT.TagReader.RuntimeViewer
{
    public class RuntimeTagLoader
    {
        public event EventHandler Completed;

        public bool
            AutoHookKey,
            AutoLoadKey,
            AutoPokeKey,
            FilterOnlyMappedKey,
            OpacityKey,
            CheckForUpdatesKey,
            AlwaysOnTopKey;
        public string ProcAsyncBaseAddr = TRSettings.Instance.ProcAsyncBaseAddr;

        public bool done_loading_settings;
        public Dictionary<string, TagStructMem> TagsList { get; set; } = new(); // and now we can convert it back because we just sort it elsewhere
        public SortedDictionary<string, GroupTagStruct> TagGroups { get; set; } = new();
        public Dictionary<string, string> InhaledTagnames = new();
        #region poop region



        /*public void ShowPointerDialog(object source, RoutedEventArgs e)
        {
            PointerDialog pointerDialog = new();
            pointerDialog.Show();
            pointerDialog.Focus();
        }*/

        public void GetGeneralSettingsFromConfig()
        {
            AutoHookKey = TRSettings.Instance.AutoHook;
            AutoLoadKey = TRSettings.Instance.AutoLoad;
            AutoPokeKey = TRSettings.Instance.AutoPoke;
            FilterOnlyMappedKey = TRSettings.Instance.FilterOnlyMapped;
            OpacityKey = TRSettings.Instance.Opacity;
            AlwaysOnTopKey = TRSettings.Instance.AlwaysOnTop;
            CheckForUpdatesKey = TRSettings.Instance.Updater;
        }
        public void SetGeneralSettingsFromConfig()
        {
            GetGeneralSettingsFromConfig();
            /*CbxSearchProcess.IsChecked = AutoHookKey;
            CbxAutoPokeChanges.IsChecked = AutoPokeKey;
            CbxFilterUnloaded.IsChecked = FilterOnlyMappedKey;
            whatdoescbxstandfor.IsChecked = AutoLoadKey; // Probably check box... -Z
            CbxOnTop.IsChecked = AlwaysOnTopKey;
            CbxOpacity.IsChecked = OpacityKey;
            CbxCheckForUpdates.IsChecked = CheckForUpdatesKey;*/
        }
        public void OnApplyChanges_Click()
        {
            SaveUserChangedSettings();
            //TRSettings.Instance.   ();
            SetGeneralSettingsFromConfig();
        }
        public void SaveUserChangedSettings()
        {/*
            TRSettings.Instance.AutoHook = CbxSearchProcess.IsChecked;
            TRSettings.Instance.AutoLoad = whatdoescbxstandfor.IsChecked;
            TRSettings.Instance.AutoPoke = CbxAutoPokeChanges.IsChecked;
            TRSettings.Instance.FilterOnlyMapped = CbxFilterUnloaded.IsChecked;
            TRSettings.Instance.AlwaysOnTop = CbxOnTop.IsChecked;
            TRSettings.Instance.Opacity = CbxOpacity.IsChecked;
            TRSettings.Instance.Updater = CbxCheckForUpdates.IsChecked;
            */
        }


        #endregion

        public delegate void HookAndLoadDelagate();
        public delegate void LoadTagsDelagate();
        private readonly System.Timers.Timer _t;
        public Mem M = new();


        //Offsets
        public string
                                // Hard-Coded Addresses
                                //HookProcessAsyncBaseAddr = "HaloInfinite.exe+0x40AD150",   //TU11
                                HookProcessAsyncBaseAddr,                                    // Tag_List_Function
                                ScanMemAOBBaseAddr = "HaloInfinite.exe+0x305B1B0",           // Tag_List_Str

                                // AOB's to scan.
                                AOBScanTagStr = "74 61 67 20 69 6E 73 74 61 6E 63 65 73"; // Tag_List_Backup Str to find
        private readonly long
                                AOBScanStartAddr = Convert.ToInt64("0000010000000000", 16),
                                AOBScanEndAddr = Convert.ToInt64("000003ffffffffff", 16);

        private bool is_checked;
        private long BaseAddress = -1;
        private int TagCount = -1;
        public bool loadedTags = false;
        public bool hooked = false;
        public long aobStart;

        public RuntimeTagLoader()
        {

        }

        public static IEnumerable<string> SplitThis(string str, int n)
        {

            return Enumerable.Range(0, str.Length / n)
                            .Select(i => str.Substring(i * n, n));
        }

        public async Task ScanMem()
        {
            // FALLBACK ADDRESS POINTER (which is literally useless)
            // However, it is faster than scanning memory and is used as a fast reference ptr to load quicker.
            BaseAddress = M.ReadLong(ScanMemAOBBaseAddr);
            string validtest = M.ReadString(BaseAddress.ToString("X"));

            if (validtest == "tag instances")
            {
                //hook_text.Text = "Process Hooked: " + M.mProc.Process.Id;
                hooked = true;
            }
            else
            {
                //hook_text.Text = "Offset failed, scanning...";
                try
                {
                    long? aobScan = (await M.AoBScan(AOBScanStartAddr, AOBScanEndAddr, AOBScanTagStr, true))
                        .First(); // "tag instances"

                    long haloInfinite = 0;
                    if (aobScan != null)
                    {
                        //get all processes named HaloInfinite
                        foreach (Process process in Process.GetProcessesByName("HaloInfinite"))
                        {
                            //get the base address of the process
                            haloInfinite = (long)process.MainModule.BaseAddress;
                        }
                        string aobHex = aobScan.Value.ToString("X");
                        IEnumerable<string> aobStr = SplitThis("0" + aobHex, 2);
                        IEnumerable<string> aobReversed = aobStr.Reverse().ToArray();
                        string aobSingle = string.Join("", aobReversed);
                        aobSingle = Regex.Replace(aobSingle, ".{2}", "$0 ");
                        aobSingle = aobSingle.TrimEnd();
                        Debugger.Log(0, "DBGTIMING", "AOB: " + aobSingle);
                        var pointers = (await M.AoBScan(haloInfinite, 140737488289791, aobSingle + " 00 00", true, true, true));
                        if (pointers != null && pointers.Count<long>() != 0) {
                            long pointer = pointers.First();
                            Debug.WriteLine(pointer);
                            TRSettings.Instance.ProcAsyncBaseAddr = "HaloInfinite.exe+0x" + (pointer - haloInfinite).ToString("X");
                        }
                        
                        //TRSettings.Instance.Save();
                        //Debug.WriteLine(TRSettings.Instance.ProcAsyncBaseAddr);

                    }



                    // Failed to find base tag address
                    if (aobScan == null || aobScan == 0)
                    {
                        BaseAddress = -1;
                        loadedTags = false;
                        //hook_text.Text = "Failed to locate base tag address";
                    }
                    else
                    {
                        BaseAddress = aobScan.Value;
                        //hook_text.Text = "Process Hooked: " + M.mProc.Process.Id + " (AOB)";
                        hooked = true;
                    }
                }
                catch (Exception)
                {
                    //hook_text.Text = "Cant find HaloInfinite.exe";
                }
            }
        }
        public bool hookProcess(Memory.Mem m) // ok since this was setup as a bool, imma use it as a bool
        {
            // true: we reconnected to the process
            // false: no new connection to process

            
            bool selected = m.OpenProcess("HaloInfinite.exe");
            return selected;
            
            

        }
        private async Task HookProcessAsync()
        {
            try
            {
                bool reset = hookProcess(M);
                if (M.mProc.Process != null) {
                    BaseAddress = -1;
                    hooked = false;
                    loadedTags = false;
                }
                if (M.mProc.Process.Handle == IntPtr.Zero || loadedTags == false) // || processSelector.selected == false
                {
                    // Could not find the process
                    //hook_text.Text = "Cant find HaloInfinite.exe";
                    BaseAddress = -1;
                    hooked = false;
                    loadedTags = false;
                    //TagsTree.Items.Clear();
                }

                if (!hooked)// || reset
                {
                    // Get the base address
                    UpdateAddress();
                    BaseAddress = M.ReadLong(HookProcessAsyncBaseAddr);
                    string validtest = M.ReadString(BaseAddress.ToString("X"));
                    //System.Diagnostics.Debug.WriteLine(M.ReadLong("HaloInfinite .exe+0x3D13E38")); // this is the wrong address lol
                    if (validtest == "tag instances")
                    {
                        //hook_text.Text = "Process Hooked: " + M.mProc.Process.Id;
                        hooked = true;
                    }
                    else
                    {
                        //hook_text.Text = "Offset failed, scanning...";
                        await ScanMem();
                    }
                }/**/
            }
            catch (Exception ex)
            {/*
                Debug.WriteLine(ex.ToString());
                //If exception is a null reference exception, set the hook text to "Game Not Open"
                if (ex.GetType().IsAssignableFrom(typeof(NullReferenceException)))
                {
                    hook_text.Text = "Can't Find HaloInfinite.exe";
                    //Show a message box that prompts the user if they want to launch the game
                    MessageBoxResult result = (MessageBoxResult)System.Windows.Forms.MessageBox.Show("HaloInfinite.exe is not open. Do you want to open it?", "HaloInfinite.exe Not Open", System.Windows.Forms.MessageBoxButtons.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        //Check if the setting for the game location is set
                        if (TRSettings.Instance.GameLocation != "")
                        {
                            //If it is set, open the game
                            System.Diagnostics.Process.Start(TRSettings.Instance.GameLocation);
                            //wait 15 seconds before trying to load again
                            await Task.Delay(15000);
                            //Try to hook again
                            await HookProcessAsync();
                        }
                        else
                        {
                            //If it is not set, allow the user to browse to an exe file, then open that exe file, wait 15 seconds, and resume loading.
                            OpenFileDialog ofd = new()
                            {
                                Filter = "HaloInfinite.exe|HaloInfinite.exe",
                                Title = "Please select HaloInfinite.exe"
                            };
                            ofd.ShowDialog();
                            TRSettings.Instance.GameLocation = ofd.FileName;
                            TRSettings.Instance.Save();
                            System.Diagnostics.Process.Start(TRSettings.Instance.GameLocation);
                            await Task.Delay(15000);
                            await HookProcessAsync();
                        }
                    }

                }*/
            }
        }
        public void UpdateAddress()
        {
            if (TRSettings.Instance.ProcAsyncBaseAddr != "undefined")
            {
                HookProcessAsyncBaseAddr = TRSettings.Instance.ProcAsyncBaseAddr;
            }
            else
            {
                HookProcessAsyncBaseAddr = "HaloInfinite.exe+0x40C3048";
            }
        }

        public async void HookAndLoad()
        {
            try
            {
                await HookProcessAsync();
            }
            catch (System.ArgumentNullException)
            {

            }
            if (BaseAddress != -1 && BaseAddress != 0)
            {
                await LoadTagsMem(false);


                if (hooked == true)
                {
                    //Searchbox_TextChanged(null, null);

                    System.Diagnostics.Debugger.Log(0, "DBGTIMING", "Done loading tags");

                }
            }
        }
        public string? read_tag_group(long tagGroupAddress)
        {
            try
            {
                string key = ReverseString(M.ReadString((tagGroupAddress + 0xC).ToString("X"), "", 8).Substring(0, 4));
                if (!TagGroups.ContainsKey(key))
                {
                    GroupTagStruct currentGroup = new()
                    {
                        TagGroupDesc = M.ReadString((tagGroupAddress).ToString("X") + ",0x0"),
                        TagGroupName = key,
                        TagGroupDefinitition = M.ReadString((tagGroupAddress + 0x20).ToString("X") + ",0x0,0x0"),
                        TagExtraType = M.ReadString((tagGroupAddress + 0x2C).ToString("X"), "", 12)
                    };

                    long testAddress = M.ReadLong((tagGroupAddress + 0x48).ToString("X"));
                    if (testAddress != 0)
                    {
                        currentGroup.TagExtraName = M.ReadString((testAddress).ToString("X"));
                    }

                    // Doing the UI here so we dont have to literally reconstruct the elements elsewhere // lol // xd how'd that work out for you
                    //TreeViewItem sortheader = new TreeViewItem();
                    //sortheader.Header = ReverseString(current_group.tag_group_name.Substring(0, 4)) + " (" + current_group.tag_group_desc + ")";
                    //sortheader.ToolTip = current_group.tag_group_definitition;
                    //TagsTree.Items.Add(sortheader);
                    //current_group.tag_category = sortheader;

                    TagGroups.Add(key, currentGroup);
                }

                return key;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string ReverseString(string myStr)
        {
            char[] myArr = myStr.ToCharArray();
            Array.Reverse(myArr);
            return new string(myArr);
        }

        public string convert_ID_to_tag_name(string value)
        {
            _ = InhaledTagnames.TryGetValue(value, value: out string? potentialName);

            return potentialName ??= "ObjectID: " + value;
        }
        public async Task LoadTagsMem(bool is_silent)
        {
            //is_checked = CbxFilterUnloaded.IsChecked;
            await Task.Run(() =>
            {

                if (TagCount != -1)
                {
                    TagCount = -1;
                    TagGroups.Clear();
                    TagsList.Clear();
                }
                TagCount = M.ReadInt((BaseAddress + 0x6C).ToString("X"));
                long tagsStart = M.ReadLong((BaseAddress + 0x78).ToString("X"));

                // each tag is 52 bytes long // was it 52 or was it 0x52? whatever
                // 0x0 datnum 4bytes
                // 0x4 ObjectID 4bytes
                // 0x8 Tag_group Pointer 8bytes
                // 0x10 Tag_data Pointer 8bytes
                // 0x18 Tag_type_desc Pointer 8bytes
               
                TagsList = new Dictionary<string, TagStructMem>();
                for (int tagIndex = 0; tagIndex < TagCount; tagIndex++)
                {
                    TagStructMem currentTag = new();
                    long tagAddress = tagsStart + (tagIndex * 52);

                    byte[] test1 = M.ReadBytes(tagAddress.ToString("X"), 4);
                    try
                    {
                        currentTag.Datnum = BitConverter.ToString(test1).Replace("-", string.Empty);
                        loadedTags = false;
                    }
                    catch (ArgumentNullException)
                    {
                        hooked = false;
                        return;
                    }
                    byte[] test = (M.ReadBytes((tagAddress + 4).ToString("X"), 4));

                    // = String.Concat(bytes.Where(c => !Char.IsWhiteSpace(c)));
                    
                    currentTag.ObjectId = BitConverter.ToString(test).Replace("-", string.Empty);
                    currentTag.TagGroupMem = read_tag_group(M.ReadLong((tagAddress + 0x8).ToString("X")));
                    currentTag.TagData = M.ReadLong((tagAddress + 0x10).ToString("X"));
                    currentTag.TagFullName = convert_ID_to_tag_name(currentTag.ObjectId).Trim();
                    currentTag.TagFile = currentTag.TagFullName.Split('\\').Last().Trim();
                    byte[] debug = M.ReadBytes((tagAddress).ToString("X"), 52);

                    var temp_s=M.ReadString(M.ReadLong((tagAddress + 0x10+24).ToString("X")).ToString("X"),"",300);
                    var temp_s1=M.ReadString(M.ReadLong((tagAddress + 0x10+32).ToString("X")).ToString("X"),"",300);
                    string str_bytes = BitConverter.ToString(debug).Replace("-", "");
                    var ind1 = BitConverter.ToInt16(debug, 0x2A);
                    var ind0 = BitConverter.ToInt16(debug, 0x2A-2);
                    if (currentTag.TagGroup != "shbc") {
                        // Debug.Assert(ind1 == 4096);
                       // Debug.Assert(ind0 == 5);
                    }
                    
                    //TagFileHeader tagFileHeaderInst = (TagFileHeader)UtilBinaryReader.marshallBinData<TagFileHeader>(tempheader);
                    if (is_checked)
                    {
                        byte[] b = M.ReadBytes((currentTag.TagData + 12).ToString("X"), 4);
                        if (b != null)
                        {
                            string checked_datnum = BitConverter.ToString(b).Replace("-", string.Empty);
                            if (checked_datnum != currentTag.Datnum)
                            {
                                currentTag.unloaded = true;
                            }
                        }
                        else
                        {
                            currentTag.unloaded = true;
                        }
                    }
                    // do the tag definitition
                    if (!TagsList.ContainsKey(currentTag.ObjectId))
                    {
                        TagsList.Add(currentTag.ObjectId, currentTag);
                    }
                    
                }
            });
            if (!is_silent)
                await Loadtags();

        }

        public bool checkLoadTagInstance(string tagID) // i love dictionaries
        {
            TagStructMem loadingTag = TagsList[tagID];
            //Tagname_text.Text = _mainWindow.convert_ID_to_tag_name(loadingTag.ObjectId);
            //tagID_text.Text = "ID: " + loadingTag.ObjectId;
            //tagdatnum_text.Text = "Datnum: " + loadingTag.Datnum;
            //tagdata_text.Text = "Tag data address: 0x" + loadingTag.TagData.ToString("X");

            //tagfilter_text.Text = "";

            //tagview_panels.Children.Clear();

            // OK, now we do a proper check to see if this tag is loaded, finally looked into this

            try // never done this before and i hope im doing it terribly wrong
            {
                // pointer check
                //TagValueBlock p_block = new() { HorizontalAlignment = HorizontalAlignment.Left };
                //p_block.value_type.Text = "Pointer check";
                var pointerCheck = M.ReadLong((loadingTag.TagData).ToString("X"));//.ToString("X");
                //tagview_panels.Children.Add(p_block);
                byte[] tempheader = M.ReadBytes((loadingTag.TagData).ToString("X"),80);
                TagFileHeader tagFileHeaderInst = (TagFileHeader)UtilBinaryReader.marshallBinData<TagFileHeader>(tempheader);
                
                // ID check
                //TagValueBlock id_block = new() { HorizontalAlignment = HorizontalAlignment.Left };
                //id_block.value_type.Text = "ID check";
                string checked_ID = BitConverter.ToString(M.ReadBytes((loadingTag.TagData + 8).ToString("X"), 4)).Replace("-", string.Empty);
                //id_block.value.Text = checked_ID;
                //tagview_panels.Children.Add(id_block);

                // Datnum check
                //TagValueBlock dat_block = new() { HorizontalAlignment = HorizontalAlignment.Left };
                //dat_block.value_type.Text = "Datnum check";
                string checked_datnum = BitConverter.ToString(M.ReadBytes((loadingTag.TagData + 12).ToString("X"), 4)).Replace("-", string.Empty);
                //dat_block.value.Text = checked_datnum;
                //tagview_panels.Children.Add(dat_block);

                if (checked_ID != loadingTag.ObjectId || checked_datnum != loadingTag.Datnum)
                {
                    //TextBox tb1 = new TextBox { Text = "Datnum/ID mismatch; Tag appears to be unloaded, meaning it may not be active on the map, else try reloading the tags" };
                    //tagview_panels.Children.Add(tb1);
                    return false; 
                }

                //if (TagLayouts.Tags.ContainsKey(loadingTag.TagGroupMem))
                //{
                //Dictionary<long, TagLayouts.C> tags = TagLayouts.Tags(loadingTag.TagGroupMem);
                //readTagsAndCreateControls(loadingTag, 0, tags, loadingTag.TagData, tagview_panels, tagID + ":");
                
                //}
                //else
                //{
                //	TextBox tb = new TextBox { Text = "This tag isn't mapped out ):" };
                //	tagview_panels.Children.Add(tb);
                //}
            }
            catch
            {
                //TextBox tb = new TextBox { Text = "ran into an oopsie woopsie, this tag is probably broken/unloaded right now" };
                //tagview_panels.Children.Add(tb);
                return false;
            }

            return true;
        }

        public async Task Loadtags()
        {
            Completed?.Invoke(this,null);

            //Dictionary<string, TreeViewItem> groups_headers_diff = new();

            await Task.Run(async () =>
            {
                /*
                // cycle through and evaluate against diff

                // act accordingly

                // save

                // TagsTree
                loadedTags = true;
                for (int i = 0; i < TagGroups.Count; i++) // per group
                {
                    KeyValuePair<string, GroupTagStruct> goop = TagGroups.ElementAt(i);

                    //ObservableCollection<GroupTagStruct> tagGroups = new(TagGroups.Values);

                    if (groups_headers.Keys.Contains(goop.Key)) // is included in group_headers
                    {

                        TreeViewItem t = groups_headers[goop.Key];
                        groups_headers_diff.Add(goop.Key, t);
                        groups_headers.Remove(goop.Key);

                        GroupTagStruct displayGroup = goop.Value;
                        displayGroup.TagCategory = t;
                        TagGroups[goop.Key] = displayGroup;

                    }
                    else
                    {
                        GroupTagStruct displayGroup = goop.Value;
                        Dispatcher.Invoke(new Action(() =>
                        {
                            TreeViewItem sortheader = new()
                            {
                                Header = displayGroup.TagGroupName + " (" + displayGroup.TagGroupDesc + ")",
                                ToolTip = new TextBlock { Foreground = Brushes.Black, Text = displayGroup.TagGroupDefinitition }
                            };
                            displayGroup.TagCategory = sortheader;
                            TagGroups[goop.Key] = displayGroup;

                            TagsTree.Items.Add(sortheader); //The tree view in the UI

                            groups_headers_diff.Add(goop.Key, sortheader);


                        }));



                    }

                }


                Dispatcher.Invoke(new Action(async () =>
                {
                    foreach (KeyValuePair<string, TreeViewItem> poop in groups_headers) // per group
                    {
                        if (poop.Value != null)
                        {
                            TagsTree.Items.Remove(poop.Value);
                        }
                    }
                    groups_headers = groups_headers_diff;
                }));


                Dictionary<string, TreeViewItem> tags_headers_diff = new();

                int iteration = 0;
                foreach (KeyValuePair<string, TagStruct> curr_tag in TagsList.OrderBy(key => key.Value.TagFullName)) // per tag
                {
                    iteration += 1;
                    if (!curr_tag.Value.unloaded)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            if (tags_headers.Keys.Contains(curr_tag.Key)) // is included in tag_headers UI
                            {
                                TreeViewItem t = tags_headers[curr_tag.Key];
                                t.Tag = curr_tag.Key;
                                tags_headers_diff.Add(curr_tag.Key, t);
                                tags_headers.Remove(curr_tag.Key);
                            }
                            else // tag isnt in UI
                            {
                                TreeViewItem t = new();
                                TagStruct tag = curr_tag.Value;
                                TagGroups.TryGetValue(tag.TagGroupMem, out GroupTagStruct? dictTagGroup);

                                t.Header = "(" + tag.Datnum + ") " + convert_ID_to_tag_name(tag.ObjectId);

                                t.Tag = curr_tag.Key; // our index to our tag

                                t.Selected += Select_Tag_click;


                                if (dictTagGroup != null && dictTagGroup.TagCategory != null)
                                {
                                    dictTagGroup.TagCategory.Items.Add(t);
                                }

                                tags_headers_diff.Add(curr_tag.Key, t);

                            }

                        }));
                        if (iteration > 200)
                        {
                            Thread.Sleep(1);
                            iteration = 0;
                        }
                    }
                }
                foreach (KeyValuePair<string, TreeViewItem> poop in tags_headers) // per tag remove
                {
                    if (poop.Value != null)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            TreeViewItem ownber = poop.Value.Parent as TreeViewItem;
                            ownber.Items.Remove(poop.Value);
                        }));
                    }
                }
                tags_headers = tags_headers_diff;


                if (TagsTree.Items.Count < 1)
                {
                    loadedTags = false;
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    hook_text.Text = "Loaded Tags";
                    //Sort the tags tree alphabetically.
                    TagsTree.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));
                }));*/
            });

        }


    }
}
