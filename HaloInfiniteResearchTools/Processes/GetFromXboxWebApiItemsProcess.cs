using LibHIRT.Grunt;
using LibHIRT.Grunt.Models.HaloInfinite;
using OpenSpartan.Grunt.Models.HaloInfinite;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using File = System.IO.File;

namespace HaloInfiniteResearchTools.Processes
{

    public class GetFromXboxWebApiItemsProcess : ProcessBase
    {
        ConnectXboxServicesResult connectXbox;
        bool _reloadInvenIfExist = false;
        private ItemType itemType;

        InventoryDefinition inventoryDefinition;
        public GetFromXboxWebApiItemsProcess(ConnectXboxServicesResult _connectXbox, ItemType itemType, bool reloadInvenIfExist = false)
        {

            connectXbox = _connectXbox;
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 4);
            //defaultInterpolatedStringHandler.AppendFormatted("/inventory/armor/themes/007-000-lone-wolf-0903655e.json");
            _reloadInvenIfExist = reloadInvenIfExist;
            this.itemType = itemType;
            // connectXbox.Client = new HaloInfiniteClientFix(connectXbox.Client.SpartanToken, connectXbox.Client.Xuid, connectXbox.Client.ClearanceToken);

        }

        protected async override Task OnExecuting()
        {

            try
            {
                //var example = await client.StatsGetMatchStats("21416434-4717-4966-9902-af7097469f74");
                var playerName = connectXbox.ExtendedTicket.DisplayClaims.Xui[0].Gamertag;
                var playerXUID = connectXbox.ExtendedTicket.DisplayClaims.Xui[0].XUID;
                var strPlayerXUID = "xuid(" + playerXUID + ")";
                string fileName = LibHIRT.Utils.Utils.CreatePathFromString("customizationCatalog.json", "", "json");
                if (true || (!File.Exists(fileName) || _reloadInvenIfExist))
                {
                    Status = "Getting customization catalog from api...";
                    var customizationCatalog = await connectXbox.Client.GameCmsGetCustomizationCatalog(connectXbox.Client.ClearanceToken);
                    if (customizationCatalog.Result != null)
                    {
                        string jsonString = JsonSerializer.Serialize(customizationCatalog.Result);
                        inventoryDefinition = customizationCatalog.Result;
                        File.WriteAllText(fileName, jsonString);
                    }
                    Status = "Saved customization catalog from api...";
                }
                else
                {
                    Status = "Getting customization catalog from disk...";
                    inventoryDefinition = (InventoryDefinition)JsonSerializer.Deserialize(fileName, typeof(InventoryDefinition));
                    Status = "loaded customization catalog from disk...";
                }
                
                
                var list =  getItempOfType(this.itemType);


                Status = list.Count > 1 ? "Opening Files" : "Opening File";
                UnitName = list.Count > 1 ? "files opened" : "file opened";
                TotalUnits = list.Count;
                IsIndeterminate = list.Count == 1;

                var objLock = new object();
                ParallelOptions parallelOptions = new ParallelOptions();
                parallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount * 4;
                await Parallel.ForEachAsync(list, parallelOptions, async (item, token )=>
                {
                    try
                    {

                        if (string.IsNullOrEmpty(item.ItemPath))
                        {

                            StatusList.AddWarning(item.ItemId, "Failed to open file.");
                        }
                        else
                        {
                            bool result = await this.SaveCmsItemToDisk(item.ItemPath);
                            if (!result)
                            {
                                StatusList.AddWarning(item.ItemId, "Failed to open file.");
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        StatusList.AddError(fileName, ex);
                    }
                    finally
                    {
                        lock (objLock)
                        {
                            CompletedUnits++;
                        }

                    }
                });

            }
            catch (Exception expe)
            {

                throw expe;
            }

        }

        private List<PlayerItem> getItempOfType(ItemType itemType)
        {
            List<PlayerItem> result = new List<PlayerItem> { };
            if (this.inventoryDefinition == null)
                return result;
            if (itemType == ItemType.All)
                return inventoryDefinition.Items;
            result = inventoryDefinition.Items.FindAll(item => item.ItemType == itemType.ToString());

            return result;

        }

        private async Task<bool> SaveCmsItemToDisk(string themepath, bool overwirte = false)
        {
            string theme_fileName = LibHIRT.Utils.Utils.CreatePathFromString(themepath, "", "json");
            if (!overwirte)
            {
                if (File.Exists(theme_fileName))
                    return true;
            }
            bool retry = true;
            int max_retry = 3;
            while (max_retry!=0)
            {
                try
                {
                    var theme_temp = await connectXbox.Client.GameCmsGetItemFix("/" + themepath.ToLower(), connectXbox.Client.ClearanceToken);

                    if (theme_temp.Error != null && theme_temp.Error.Code == 200)
                    {

                        //string jsonString = JsonSerializer.Serialize(theme_wlv_c13d0b38.Result);
                        string jsonString_temp = theme_temp.Error.Message;
                        System.IO.File.WriteAllText(theme_fileName, jsonString_temp);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    max_retry--;
                    if (max_retry == 0)
                    {
                        throw ex; 
                    }
                    
                }
            }
            
            return false;
        }


    }


}
