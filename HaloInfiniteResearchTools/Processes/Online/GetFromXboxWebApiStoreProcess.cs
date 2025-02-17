using LibHIRT.Grunt;
using LibHIRT.Grunt.Models.HaloInfinite;
using Newtonsoft.Json.Linq;
using OpenSpartan.Grunt.Models.HaloInfinite;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using File = System.IO.File;

namespace HaloInfiniteResearchTools.Processes.Online
{

    public class GetFromXboxWebApiStoreProcess : ProcessBase
    {
        ConnectXboxServicesResult connectXbox;
        bool _reloadInvenIfExist = false;
        private StoreId storeId;

        InventoryDefinition inventoryDefinition;
        public GetFromXboxWebApiStoreProcess(ConnectXboxServicesResult _connectXbox, StoreId storeId = StoreId.all, bool reloadInvenIfExist = false)
        {

            connectXbox = _connectXbox;
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 4);
            //defaultInterpolatedStringHandler.AppendFormatted("/inventory/armor/themes/007-000-lone-wolf-0903655e.json");
            _reloadInvenIfExist = reloadInvenIfExist;
            this.storeId = storeId;
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


                Status = "Getting customization catalog from api...";
                List<StoreId> list = new List<StoreId>();
                if (this.storeId == StoreId.all)
                {
                    foreach (StoreId item in Enum.GetValues(typeof(StoreId)))
                    {
                        if (item == StoreId.all)
                            continue;
                        list.Add(item);
                    }
                }
                else
                {
                    list.Add(this.storeId);
                }

                Status = list.Count > 1 ? "Opening Files" : "Opening File";
                UnitName = list.Count > 1 ? "files opened" : "file opened";
                TotalUnits = list.Count;
                IsIndeterminate = false;
                foreach (var item in list)
                {
                    try
                    {
                        var customizationCatalog = await connectXbox.Client.GameCmsGetStoreBy(connectXbox.Client.Xuid, item);

                        if (customizationCatalog.Result != null)
                        {
                            string jsonString = customizationCatalog.Result;
                            JObject jsonObj = JObject.Parse(jsonString);
                            string fileName = LibHIRT.Utils.Utils.CreatePathFromString((string)jsonObj["StorefrontDisplayPath"], "", "json")?.Replace(".json", "_content.json");
                            if (true || (!File.Exists(fileName) || _reloadInvenIfExist))
                            {
                                //inventoryDefinition = customizationCatalog.Result;
                                File.WriteAllText(fileName, jsonString);
                            }

                            var test = await connectXbox.Client.GameCmsGetItemJsonStringFix((string)jsonObj["StorefrontDisplayPath"], "");
                            if (test.Result != null)
                            {
                                string fileNameMain = LibHIRT.Utils.Utils.CreatePathFromString((string)jsonObj["StorefrontDisplayPath"], "", "json");
                                if (true || !File.Exists(fileNameMain) || _reloadInvenIfExist)
                                {
                                    //inventoryDefinition = customizationCatalog.Result;
                                    File.WriteAllText(fileNameMain, test.Result);
                                }
                                await GetAllStoresOffering((JArray)jsonObj["Offerings"]);
                            }
                        }
                        Status = "Saved customization catalog from api...";
                    }
                    catch (Exception ex)
                    {
                        this.StatusList.AddError(ex.Message, ex);
                        //throw ex;
                    }
                }


            }
            catch (Exception expe)
            {
                this.StatusList.AddError(expe.Message, expe);
                // throw expe;
            }

        }

        private async Task GetAllStoresOffering(JArray? list)
        {
            TotalUnits = TotalUnits + list.Count;
            IsIndeterminate = false;

            var objLock = new object();
            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount * 4;
            await Parallel.ForEachAsync(list, parallelOptions, async (jsonObj, token) =>
            {
                string name = "";
                try
                {
                    string fileNameMain = LibHIRT.Utils.Utils.CreatePathFromString((string)jsonObj["OfferingDisplayPath"], "", "json");
                    if ((!File.Exists(fileNameMain) || _reloadInvenIfExist))
                    {
                        name = (string)jsonObj["OfferingDisplayPath"];
                        var test = await connectXbox.Client.GameCmsGetItemJsonStringFix((string)jsonObj["OfferingDisplayPath"], "");

                        if (test.Result != null)
                        {
                            File.WriteAllText(fileNameMain, test.Result);
                        }
                        this.GetAllOfferingIncludedItems((JArray)jsonObj["IncludedItems"]);
                    }

                    Status = "Saved customization catalog from api...";
                    TotalUnits = TotalUnits - 1;
                }
                catch (Exception ex)
                {
                    this.StatusList.AddError(name, ex);
                    //throw ex;
                }

            });
        }
        private async Task GetAllOfferingIncludedItems(JArray? list)
        {

            TotalUnits = TotalUnits + list.Count;
            IsIndeterminate = false;

            var objLock = new object();
            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount * 4;
            await Parallel.ForEachAsync(list, parallelOptions, async (jsonObj, token) =>
            {
                string name = "";
                try
                {
                    await this.SaveCmsItemToDisk((string)jsonObj["ItemPath"]);

                    Status = "Saved tems from api...";
                    TotalUnits = TotalUnits - 1;
                }
                catch (Exception ex)
                {
                    this.StatusList.AddError(name, ex);
                    //throw ex;
                }

            });
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
            while (max_retry != 0)
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
