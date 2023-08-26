using OpenSpartan.Grunt.Models.HaloInfinite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{

    public class GetFromXboxWebApiProcess : ProcessBase
    {
        ConnectXboxServicesResult connectXbox;

        public GetFromXboxWebApiProcess(ConnectXboxServicesResult _connectXbox)
        {
            
            connectXbox = _connectXbox;
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 4);
            defaultInterpolatedStringHandler.AppendFormatted("/inventory/armor/themes/007-000-lone-wolf-0903655e.json");
            // connectXbox.Client = new HaloInfiniteClientFix(connectXbox.Client.SpartanToken, connectXbox.Client.Xuid, connectXbox.Client.ClearanceToken);
            
        }

        protected async override Task OnExecuting()
        {

            Task.Run(async () =>
            {
                try
                {
                    //var example = await client.StatsGetMatchStats("21416434-4717-4966-9902-af7097469f74");
                    var playerName = connectXbox.ExtendedTicket.DisplayClaims.Xui[0].Gamertag;
                    var playerXUID = connectXbox.ExtendedTicket.DisplayClaims.Xui[0].XUID;
                    var strPlayerXUID = "xuid(" + playerXUID + ")";

                    var example = await connectXbox.Client.EconomyPlayerOperations(strPlayerXUID);
                    bool all_cores = await GetAllArmorCoresOfPlayer(strPlayerXUID);
                    var bodyCustomization = await connectXbox.Client.EconomySpartanBodyCustomization(strPlayerXUID);
                    if (bodyCustomization.Result != null)
                    {
                        string fileName = LibHIRT.Utils.Utils.CreatePathFromString("bodyCustomization.json", "", "json");
                        string jsonString = JsonSerializer.Serialize(bodyCustomization.Result);
                        System.IO.File.WriteAllText(fileName, jsonString);
                    }
                    var allOwnedCoresDetails = await connectXbox.Client.EconomyAllOwnedCoresDetails(strPlayerXUID);
                    if (allOwnedCoresDetails.Result != null)
                    {
                        string fileName = LibHIRT.Utils.Utils.CreatePathFromString("allOwnedCoresDetails.json", "", "json");
                        string jsonString = JsonSerializer.Serialize(allOwnedCoresDetails.Result);
                        System.IO.File.WriteAllText(fileName, jsonString);
                    }




                    var gamecmsgetmetadata = await connectXbox.Client.GameCmsGetMetadata(connectXbox.Client.ClearanceToken);
                    if (gamecmsgetmetadata.Result != null)
                    {
                        string fileName = LibHIRT.Utils.Utils.CreatePathFromString("gamecmsgetmetadata.json", "", "json");
                        string jsonString = JsonSerializer.Serialize(gamecmsgetmetadata.Result);
                        System.IO.File.WriteAllText(fileName, jsonString);
                    }

                    var wlv_c13d0b38 = await connectXbox.Client.EconomyArmorCoreCustomization(strPlayerXUID, "017-001-wlv-c13d0b38");
                    if (wlv_c13d0b38.Result != null)
                    {
                        string fileName = LibHIRT.Utils.Utils.CreatePathFromString("017-001-wlv-c13d0b38.json", "", "json");
                        string jsonString = JsonSerializer.Serialize(wlv_c13d0b38.Result);
                        System.IO.File.WriteAllText(fileName, jsonString);
                    }

                   
                }
                catch (Exception expe)
                {

                    throw expe;
                }


                /* 017-001-wlv-c13d0b38
                 * Inventory/Armor/Themes/007-000-lone-wolf-0903655e.json
                if (_responseThread.IsAlive)
                {
                    do_not_stop_listener = false;
                }*/

                Debug.WriteLine("You have stats.");
            }).GetAwaiter().GetResult();
        }

        private async Task<bool> GetAllArmorCoresOfPlayer(string strPlayerXUID)
        {
            var armorCoresCustomization = await connectXbox.Client.EconomyArmorCoresCustomization(strPlayerXUID);

            if (armorCoresCustomization.Result != null)
            {
                string fileName = LibHIRT.Utils.Utils.CreatePathFromString("armorCoresCustomization.json", "", "json");
                string jsonString = JsonSerializer.Serialize(armorCoresCustomization.Result);
                System.IO.File.WriteAllText(fileName, jsonString);
                foreach (var item in armorCoresCustomization.Result.ArmorCores)
                {
                    //var theme_wlv_c13d0b38 = await connectXbox.Client.GameCmsGetItem("/inventory/armor/themes/007-000-lone-wolf-0903655e.json", connectXbox.Client.ClearanceToken);
                    bool all_Save = false;
                    
                    all_Save = await SaveCmsArmorThemeToDisk(item.Themes[0].ThemePath);

                    all_Save = await SaveCmsItemToDisk( item.Themes[0].CoatingPath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].VisorPath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].ArmorFxPath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].GlovePath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].ChestAttachmentPath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].HelmetAttachmentPath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].HelmetPath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].HipAttachmentPath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].KneePadPath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].MythicFxPath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].RightShoulderPadPath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].LeftShoulderPadPath);
                    all_Save = await SaveCmsItemToDisk( item.Themes[0].WristAttachmentPath);

                }
            }

            

            


            return true;
        }

        private async Task<bool> SaveCmsItemToDisk(string themepath)
        {
            var theme_temp = await connectXbox.Client.GameCmsGetItemFix("/" + themepath.ToLower(), connectXbox.Client.ClearanceToken);

            if (theme_temp.Error != null && theme_temp.Error.Code == 200)
            {
                string theme_fileName = LibHIRT.Utils.Utils.CreatePathFromString(themepath, "", "json");
                //string jsonString = JsonSerializer.Serialize(theme_wlv_c13d0b38.Result);
                string jsonString_temp = theme_temp.Error.Message;
                System.IO.File.WriteAllText(theme_fileName, jsonString_temp);
                return true;
            }
            return false;
        }
        
        private async Task<bool> SaveCmsArmorThemeToDisk(string themepath, bool save_options_paths= false)
        {
            var theme_temp = await connectXbox.Client.GameCmsGetArmorTheme("/" + themepath.ToLower(), connectXbox.Client.ClearanceToken);
            bool error = false;
            bool all_Save = false;
            if (theme_temp.Error != null && theme_temp.Error.Code == 200)
            {
                string theme_fileName = LibHIRT.Utils.Utils.CreatePathFromString(themepath, "", "json");
                //string jsonString = JsonSerializer.Serialize(theme_wlv_c13d0b38.Result);
                string jsonString_temp = theme_temp.Error.Message;
                System.IO.File.WriteAllText(theme_fileName, jsonString_temp);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.Coatings.DefaultOptionPath);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.Helmets.DefaultOptionPath);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.Visors.DefaultOptionPath);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.LeftShoulderPads.DefaultOptionPath);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.RightShoulderPads.DefaultOptionPath);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.Gloves.DefaultOptionPath);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.KneePads.DefaultOptionPath);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.ChestAttachments.DefaultOptionPath);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.WristAttachments.DefaultOptionPath);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.HipAttachments.DefaultOptionPath);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.ArmorFx.DefaultOptionPath);
                all_Save = await SaveCmsItemToDisk(theme_temp.Result.MythicFx.DefaultOptionPath);

                
                if (save_options_paths) {
                    error = await SaveCmsOptionPathsToDisk(theme_temp.Result.Visors.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(theme_temp.Result.ArmorFx.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(theme_temp.Result.ChestAttachments.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(theme_temp.Result.Coatings.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(theme_temp.Result.Gloves.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(theme_temp.Result.HipAttachments.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(theme_temp.Result.KneePads.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(theme_temp.Result.LeftShoulderPads.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(theme_temp.Result.RightShoulderPads.OptionPaths);
                    error = await SaveCmsOptionPathsToDisk(theme_temp.Result.WristAttachments.OptionPaths);
                }
                
                
                   
            }
            return error;
        } 
        
        private async Task<bool> SaveCmsOptionPathsToDisk(List<string> paths)
        {
            bool error = false;
            if (paths == null)
                return false;
            foreach (var item in paths)
            {
                error = await SaveCmsItemToDisk(item);
            }
            return error;
        }
    }

   
}
