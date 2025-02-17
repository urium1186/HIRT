using HaloInfiniteResearchTools.Models;
using LibHIRT.Grunt;
using OpenSpartan.Grunt.Models.HaloInfinite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes.Online
{

    public class XboxWebApiArmorCoresProcess : ProcessBase<Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>>>
    {
        public enum XboxWebApiArmorCoresAction
        {
            GetAllCurrentArmorCore,
            GetCurrentArmorCore,
            PutCurrentArmorCore,
            LoadItemsOfArmorCore,
            LoadItemsOfTheme
        }

        ConnectXboxServicesResult connectXbox;
        private bool overwrite;
        private bool savePicture;
        private XboxWebApiArmorCoresAction _armorCoresAction = XboxWebApiArmorCoresAction.GetAllCurrentArmorCore;
        private object[] _args;
        private Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>> all_cores;

        public override Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>> Result => all_cores;

        public object[] Args { get => _args; set => _args = value; }
        public XboxWebApiArmorCoresAction ArmorCoresAction { get => _armorCoresAction; set => _armorCoresAction = value; }

        public XboxWebApiArmorCoresProcess(ConnectXboxServicesResult _connectXbox, bool _overwrite = false, bool _savePicture = true, XboxWebApiArmorCoresAction armorCoresAction = XboxWebApiArmorCoresAction.GetAllCurrentArmorCore, params object[] args)
        {
            overwrite = _overwrite;
            savePicture = _savePicture;
            connectXbox = _connectXbox;
            _armorCoresAction = armorCoresAction;
            _args = args;

        }

        protected async override Task OnExecuting()
        {

            if (connectXbox == null)
                return;
            var playerXUID = connectXbox.ExtendedTicket.DisplayClaims.Xui[0].XUID;
            var strPlayerXUID = "xuid(" + playerXUID + ")";
            switch (_armorCoresAction)
            {
                case XboxWebApiArmorCoresAction.GetAllCurrentArmorCore:
                    GetAllCurrentArmorCores();
                    break;
                case XboxWebApiArmorCoresAction.GetCurrentArmorCore:
                    if (_args != null && _args.Length == 1)
                    {
                        var core = await GetArmorCoreOfPlayer(strPlayerXUID, (string)_args[0]);
                        all_cores = new Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>>();
                        all_cores[core] = null;
                    }

                    break;
                case XboxWebApiArmorCoresAction.PutCurrentArmorCore:
                    if (_args != null && _args.Length == 1)
                    {
                        ArmorCore coreToSave = (ArmorCore)_args[0];
                        var core = await PutArmorCoreOfPlayer(strPlayerXUID, coreToSave.CoreId, coreToSave);
                        all_cores = new Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>>();
                        all_cores[core] = null;
                    }

                    break;
                case XboxWebApiArmorCoresAction.LoadItemsOfArmorCore:
                    if (_args != null && _args.Length == 1)
                    {
                        Dictionary<string, OnlineItemModel> temp_theme = await SaveCmsArmorThemeToDisk(((ArmorCore)_args[0])?.Themes?[0]);
                        all_cores = new Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>>();
                        all_cores[(ArmorCore)_args[0]] = temp_theme;
                    }
                    break;
                case XboxWebApiArmorCoresAction.LoadItemsOfTheme:
                    if (_args != null && _args.Length == 1)
                    {
                        Dictionary<string, OnlineItemModel> temp_theme = await SaveCmsArmorThemeToDisk(((ArmorCoreTheme)_args[0]));
                        all_cores = new Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>>();
                        ArmorCore key = new ArmorCore();
                        all_cores[key] = temp_theme;
                    }
                    break;
                default:
                    break;
            }

        }

        private void GetAllCurrentArmorCores()
        {
            Task.Run(async () =>
            {
                try
                {

                    var playerName = connectXbox.ExtendedTicket.DisplayClaims.Xui[0].Gamertag;
                    var playerXUID = connectXbox.ExtendedTicket.DisplayClaims.Xui[0].XUID;
                    var strPlayerXUID = "xuid(" + playerXUID + ")";


                    all_cores = await GetAllArmorCoresOfPlayer(strPlayerXUID);
                    var bodyCustomization = await connectXbox.Client.EconomySpartanBodyCustomization(strPlayerXUID);
                    if (bodyCustomization.Result != null)
                    {
                        string fileName = LibHIRT.Utils.Utils.CreatePathFromString("bodyCustomization.json", "", "json");
                        string jsonString = JsonSerializer.Serialize(bodyCustomization.Result);
                        System.IO.File.WriteAllText(fileName, jsonString);
                    }
                }
                catch (Exception expe)
                {

                    throw expe;
                }

                Debug.WriteLine("You have stats.");
            }).GetAwaiter().GetResult();
        }


        private async Task<Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>>> GetAllArmorCoresOfPlayer(string strPlayerXUID, bool getThemes = false)
        {
            Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>> result = new Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>>();
            var armorCoresCustomization = await connectXbox.Client.EconomyArmorCoresCustomization(strPlayerXUID);

            if (armorCoresCustomization.Result != null)
            {
                string fileName = LibHIRT.Utils.Utils.CreatePathFromString("armorCoresCustomization.json", "", "json");
                string jsonString = JsonSerializer.Serialize(armorCoresCustomization.Result);
                System.IO.File.WriteAllText(fileName, jsonString);
                foreach (var item in armorCoresCustomization.Result.ArmorCores)
                {
                    result[item] = null;
                    if (getThemes)
                    {
                        Dictionary<string, OnlineItemModel> temp_theme = await SaveCmsArmorThemeToDisk(item?.Themes?[0]);
                        result[item] = temp_theme;
                    }
                }
            }

            return result;
        }
        private async Task<ArmorCore> GetArmorCoreOfPlayer(string strPlayerXUID, string coreId)
        {
            var core_online = await connectXbox.Client.EconomyArmorCoreCustomization(strPlayerXUID, coreId);// "017-001-wlv-c13d0b38"
            if (core_online.Result != null)
            {

            }

            return core_online?.Result;
        }

        private async Task<ArmorCore> PutArmorCoreOfPlayer(string strPlayerXUID, string coreId, ArmorCore armorCore)
        {
            string jsonString = JsonSerializer.Serialize(armorCore);
            var result_put = await connectXbox.Client.EconomyArmorCoreCustomization(strPlayerXUID, coreId, jsonString);
            return result_put?.Result;
        }

        private async Task<Dictionary<string, OnlineItemModel>> SaveCmsArmorThemeToDisk(ArmorCoreTheme? armorCoreTheme)
        {
            Dictionary<string, OnlineItemModel> result = new Dictionary<string, OnlineItemModel>();
            bool error = false;



            if (armorCoreTheme != null)
            {
                OnlineItemModel Coating = await SaveCmsItemToDisk(armorCoreTheme.CoatingPath, overwrite, savePicture);
                OnlineItemModel Helmet = await SaveCmsItemToDisk(armorCoreTheme.HelmetPath, overwrite, savePicture);
                OnlineItemModel Visor = await SaveCmsItemToDisk(armorCoreTheme.VisorPath, overwrite, savePicture);
                OnlineItemModel LeftShoulderPad = await SaveCmsItemToDisk(armorCoreTheme.LeftShoulderPadPath, overwrite, savePicture);
                OnlineItemModel RightShoulderPad = await SaveCmsItemToDisk(armorCoreTheme.RightShoulderPadPath, overwrite, savePicture);
                OnlineItemModel Glove = await SaveCmsItemToDisk(armorCoreTheme.GlovePath, overwrite, savePicture);
                OnlineItemModel KneePad = await SaveCmsItemToDisk(armorCoreTheme?.KneePadPath, overwrite, savePicture);
                OnlineItemModel ChestAttachment = await SaveCmsItemToDisk(armorCoreTheme?.ChestAttachmentPath, overwrite, savePicture);
                OnlineItemModel WristAttachment = await SaveCmsItemToDisk(armorCoreTheme?.WristAttachmentPath, overwrite, savePicture);
                OnlineItemModel HipAttachment = await SaveCmsItemToDisk(armorCoreTheme?.HipAttachmentPath, overwrite, savePicture);
                OnlineItemModel ArmorFx = await SaveCmsItemToDisk(armorCoreTheme?.ArmorFxPath, overwrite, savePicture);
                OnlineItemModel MythicFx = await SaveCmsItemToDisk(armorCoreTheme?.MythicFxPath, overwrite, savePicture);

                Coating.Description = string.IsNullOrEmpty(Coating.Description) ? "Coating" : Coating.Description;
                Helmet.Description = string.IsNullOrEmpty(Helmet.Description) ? "Helmet" : Helmet.Description;
                Visor.Description = string.IsNullOrEmpty(Visor.Description) ? "Visor" : Visor.Description;
                LeftShoulderPad.Description = string.IsNullOrEmpty(LeftShoulderPad.Description) ? "LeftShoulderPad" : LeftShoulderPad.Description;
                RightShoulderPad.Description = string.IsNullOrEmpty(RightShoulderPad.Description) ? "RightShoulderPad" : RightShoulderPad.Description;
                Glove.Description = string.IsNullOrEmpty(Glove.Description) ? "Glove" : Glove.Description;
                KneePad.Description = string.IsNullOrEmpty(KneePad.Description) ? "KneePad" : KneePad.Description;
                ChestAttachment.Description = string.IsNullOrEmpty(ChestAttachment.Description) ? "ChestAttachment" : ChestAttachment.Description;
                WristAttachment.Description = string.IsNullOrEmpty(WristAttachment.Description) ? "WristAttachment" : WristAttachment.Description;
                HipAttachment.Description = string.IsNullOrEmpty(HipAttachment.Description) ? "HipAttachment" : HipAttachment.Description;
                ArmorFx.Description = string.IsNullOrEmpty(ArmorFx.Description) ? "ArmorFx" : ArmorFx.Description;
                MythicFx.Description = string.IsNullOrEmpty(MythicFx.Description) ? "MythicFx" : MythicFx.Description;

                result["Coating"] = Coating;
                result["Helmet"] = Helmet;
                result["Visor"] = Visor;
                result["LeftShoulderPad"] = LeftShoulderPad;
                result["RightShoulderPad"] = RightShoulderPad;
                result["Glove"] = Glove;
                result["KneePad"] = KneePad;
                result["ChestAttachment"] = ChestAttachment;
                result["WristAttachment"] = WristAttachment;
                result["HipAttachment"] = HipAttachment;
                result["ArmorFx"] = ArmorFx;
                result["MythicFx"] = MythicFx;
            }

            return result;
        }

        private async Task<OnlineItemModel> SaveCmsItemToDisk(string item_path, bool overwrite = false, bool savePicture = false)
        {
            OnlineItemModel result = new OnlineItemModel();
            string jsonString_temp = "";
            string item_fileName = LibHIRT.Utils.Utils.CreatePathFromString(item_path, "", "json");
            InGameItem item = null;
            if (!overwrite && System.IO.File.Exists(item_fileName))
            {
                jsonString_temp = System.IO.File.ReadAllText(item_fileName);
                item = JsonSerializer.Deserialize<InGameItem>(jsonString_temp, connectXbox.Client.SerializerOptions);
            }
            else
            {
                var theme_temp = await connectXbox.Client.GameCmsGetItemFix("/" + item_path.ToLower(), connectXbox.Client.ClearanceToken);

                if (theme_temp.Error != null && theme_temp.Error.Code == 200)
                {
                    item = theme_temp.Result;
                    jsonString_temp = theme_temp.Error.Message;
                    System.IO.File.WriteAllText(item_fileName, jsonString_temp);
                }
                else
                    return result;
            }

            if (item != null && item.CommonData != null)
            {
                if (savePicture)
                {
                    DisplayPath tempDisplay = item.CommonData.DisplayPath;
                    string img_path = await SaveCmsImageToDisk(tempDisplay);
                    if (!string.IsNullOrEmpty(img_path))
                    {
                        result.ImageSource = img_path;
                    }
                }
                result.Description = item?.CommonData?.Title?.Value;
                return result;
            }
            else
                return result;
        }

        private async Task<string> SaveCmsImageToDisk(DisplayPath displayPath)
        {
            string pict_path = "";
            if (displayPath.FolderPath != null && displayPath.FileName != null)
                pict_path = displayPath.FolderPath + "/" + displayPath.FileName;
            else
            {
                if (string.IsNullOrEmpty(displayPath?.Media?.MediaUrl?.Path))
                {
                    return pict_path;
                }
                pict_path = displayPath?.Media?.MediaUrl?.Path;
            }

            string theme_fileName = LibHIRT.Utils.Utils.CreatePathFromString(pict_path, "", "json");
            if (System.IO.File.Exists(theme_fileName))
                return theme_fileName;
            var pict = await connectXbox.Client.GameCmsGetImage("/" + pict_path);

            if (pict != null && pict.Result != null)
            {
                System.IO.File.WriteAllBytes(theme_fileName, pict.Result);
                return theme_fileName;
            }
            return "";
        }

        private async Task<Dictionary<string, OnlineItemModel>> SaveCmsArmorThemeToDisk(string themepath, bool save_options_paths = false)
        {
            Dictionary<string, OnlineItemModel> result = new Dictionary<string, OnlineItemModel>();
            var theme_temp = await connectXbox.Client.GameCmsGetArmorTheme("/" + themepath.ToLower(), connectXbox.Client.ClearanceToken);
            if (theme_temp.Error != null && theme_temp.Error.Code == 200)
            {
                string theme_fileName = LibHIRT.Utils.Utils.CreatePathFromString(themepath, "", "json");
                string jsonString_temp = theme_temp.Error.Message;
                System.IO.File.WriteAllText(theme_fileName, jsonString_temp);
                result = await SaveCmsArmorThemeToDisk(theme_temp.Result, save_options_paths);

            }
            return result;
        }
        private async Task<Dictionary<string, OnlineItemModel>> SaveCmsArmorThemeToDisk(ArmorTheme armorTheme, bool save_options_paths = false)
        {
            Dictionary<string, OnlineItemModel> result = new Dictionary<string, OnlineItemModel>();
            bool error = false;



            if (armorTheme != null)
            {
                DisplayPath tempDisplay = armorTheme.CommonData.DisplayPath;
                string havePict = await SaveCmsImageToDisk(tempDisplay);
                if (string.IsNullOrEmpty(havePict) && armorTheme.CommonData.ParentPaths?.Count > 0)
                {
                    OnlineItemModel themeItemModel = await SaveCmsItemToDisk(armorTheme.CommonData.ParentPaths[0]?.Path, true, true);
                }



                //string jsonString = JsonSerializer.Serialize(theme_wlv_c13d0b38.Result);


                OnlineItemModel Coating = await SaveCmsItemToDisk(armorTheme.Coatings.DefaultOptionPath, overwrite, savePicture);
                OnlineItemModel Helmet = await SaveCmsItemToDisk(armorTheme.Helmets.DefaultOptionPath, overwrite, savePicture);
                OnlineItemModel Visor = await SaveCmsItemToDisk(armorTheme.Visors.DefaultOptionPath, overwrite, savePicture);
                OnlineItemModel LeftShoulderPad = await SaveCmsItemToDisk(armorTheme.LeftShoulderPads.DefaultOptionPath, overwrite, savePicture);
                OnlineItemModel RightShoulderPad = await SaveCmsItemToDisk(armorTheme.RightShoulderPads.DefaultOptionPath, overwrite, savePicture);
                OnlineItemModel Glove = await SaveCmsItemToDisk(armorTheme.Gloves.DefaultOptionPath, overwrite, savePicture);
                OnlineItemModel KneePad = await SaveCmsItemToDisk(armorTheme?.KneePads.DefaultOptionPath, overwrite, savePicture);
                OnlineItemModel ChestAttachment = await SaveCmsItemToDisk(armorTheme?.ChestAttachments.DefaultOptionPath, overwrite, savePicture);
                OnlineItemModel WristAttachment = await SaveCmsItemToDisk(armorTheme?.WristAttachments.DefaultOptionPath, overwrite, savePicture);
                OnlineItemModel HipAttachment = await SaveCmsItemToDisk(armorTheme?.HipAttachments.DefaultOptionPath, overwrite, savePicture);
                OnlineItemModel ArmorFx = await SaveCmsItemToDisk(armorTheme?.ArmorFx.DefaultOptionPath, overwrite, savePicture);
                OnlineItemModel MythicFx = await SaveCmsItemToDisk(armorTheme?.MythicFx.DefaultOptionPath, overwrite, savePicture);

                result["Coating"] = Coating;
                result["Helmet"] = Helmet;
                result["Visor"] = Visor;
                result["LeftShoulderPad"] = LeftShoulderPad;
                result["RightShoulderPad"] = RightShoulderPad;
                result["Glove"] = Glove;
                result["KneePad"] = KneePad;
                result["ChestAttachment"] = ChestAttachment;
                result["WristAttachment"] = WristAttachment;
                result["HipAttachment"] = HipAttachment;
                result["ArmorFx"] = ArmorFx;
                result["MythicFx"] = MythicFx;

                if (save_options_paths)
                {
                    error = await SaveCmsOptionPathsToDisk(armorTheme?.Visors.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(armorTheme?.ArmorFx.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(armorTheme?.ChestAttachments.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(armorTheme?.Coatings.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(armorTheme?.Gloves.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(armorTheme?.HipAttachments.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(armorTheme?.KneePads.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(armorTheme?.LeftShoulderPads.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(armorTheme?.RightShoulderPads.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(armorTheme?.WristAttachments.OptionPaths);
                }



            }
            return result;
        }

        private async Task<bool> SaveCmsOptionPathsToDisk(List<string> paths)
        {
            OnlineItemModel error = null;
            if (paths == null)
                return false;
            foreach (var item in paths)
            {
                error = await SaveCmsItemToDisk(item, true);
            }
            return error != null;
        }
    }


}
