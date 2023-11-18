using LibHIRT.Grunt.Converters;
using OpenSpartan.Grunt.Models.HaloInfinite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{

    public class GetArmorCoresFromJsonProcess : ProcessBase<List<ArmorCore>>
    {
        string _json_path = "";
        private List<ArmorCore> _listArmorCores = null;

        public Dictionary<string, string> CmsJsonPair { get; set; }

        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                (JsonConverter)new EmptyDateStringToNullJsonConverter(),
                (JsonConverter)new OnlineUriReferenceConverter(),
                (JsonConverter)new AcknowledgementTypeConverter(),
                (JsonConverter)new XmlDurationToTimeSpanJsonConverter()
            }
        };

        public GetArmorCoresFromJsonProcess(string _json_path = "")
        {
            _json_path = _json_path;
        }

        public override List<ArmorCore> Result => _listArmorCores;

        protected async override Task OnExecuting()
        {

            Task.Run(async () =>
            {
                try
                {
                    //var example = await client.StatsGetMatchStats("21416434-4717-4966-9902-af7097469f74");
                    if (CmsJsonPair == null)
                        CmsJsonPair = new Dictionary<string, string>();
                    else
                        CmsJsonPair.Clear();

                    bool all_cores = await GetAllArmorCoresOfPlayer();


                }
                catch (Exception expe)
                {

                    throw expe;
                }

                Debug.WriteLine("You have stats.");
            }).GetAwaiter().GetResult();
        }

        private async Task<bool> GetAllArmorCoresOfPlayer()
        {
            ArmorCoreCollection result = (ArmorCoreCollection)await LoadCmsItemFromDisk("armorCoresCustomization.json", typeof(ArmorCoreCollection));
            BodyCustomization bodyCustomization = (BodyCustomization)await LoadCmsItemFromDisk("bodyCustomization.json", typeof(BodyCustomization));

            if (result != null)
            {
                _listArmorCores = result.ArmorCores;
                foreach (var item in _listArmorCores)
                {
                    //var theme_wlv_c13d0b38 = await connectXbox.Client.GameCmsGetItem("/inventory/armor/themes/007-000-lone-wolf-0903655e.json", connectXbox.Client.ClearanceToken);
                    bool all_Save = false;

                    all_Save = await LoadCmsArmorThemeFromDisk(item.Themes[0].ThemePath);
                    string s_result = "";
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].CoatingPath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].VisorPath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].ArmorFxPath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].GlovePath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].ChestAttachmentPath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].HelmetAttachmentPath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].HelmetPath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].HipAttachmentPath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].KneePadPath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].MythicFxPath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].RightShoulderPadPath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].LeftShoulderPadPath);
                    s_result = await LoadCmsItemFromDisk(item.Themes[0].WristAttachmentPath);

                }
            }

            return true;
        }

        private async Task<string> LoadCmsItemFromDisk(string themepath)
        {

            string full_path = LibHIRT.Utils.Utils.CreatePathFromString(themepath, "", "json");

            if (System.IO.File.Exists(full_path))
            {
                string jsonString_temp = System.IO.File.ReadAllText(full_path);
                CmsJsonPair[themepath] = jsonString_temp;
                return jsonString_temp;
            }
            return "";
        }

        private async Task<object> LoadCmsItemFromDisk(string themepath, Type ret_type)
        {

            string full_path = LibHIRT.Utils.Utils.CreatePathFromString(themepath, "", "json");

            if (System.IO.File.Exists(full_path))
            {
                string jsonString_temp = System.IO.File.ReadAllText(full_path);
                CmsJsonPair[themepath] = jsonString_temp;
                return JsonSerializer.Deserialize(jsonString_temp, ret_type, serializerOptions);
            }
            return null;
        }

        private async Task<bool> LoadCmsArmorThemeFromDisk(string themepath, bool save_options_paths = false)
        {
            bool error = false;
            ArmorTheme theme_temp = (ArmorTheme)await LoadCmsItemFromDisk(themepath, typeof(ArmorTheme));


            bool all_Save = false;
            if (theme_temp != null)
            {
                string theme_fileName = LibHIRT.Utils.Utils.CreatePathFromString(themepath, "", "json");
                //string jsonString = JsonSerializer.Serialize(theme_wlv_c13d0b38.Result);

                string all_Save_ = "";
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.Coatings.DefaultOptionPath);
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.Helmets.DefaultOptionPath);
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.Visors.DefaultOptionPath);
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.LeftShoulderPads.DefaultOptionPath);
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.RightShoulderPads.DefaultOptionPath);
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.Gloves.DefaultOptionPath);
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.KneePads.DefaultOptionPath);
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.ChestAttachments.DefaultOptionPath);
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.WristAttachments.DefaultOptionPath);
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.HipAttachments.DefaultOptionPath);
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.ArmorFx.DefaultOptionPath);
                all_Save_ = await LoadCmsItemFromDisk(theme_temp.MythicFx.DefaultOptionPath);


                /*if (save_options_paths) {
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
                }*/



            }
            return error;
        }

        private async Task<bool> SaveCmsOptionPathsToDisk(List<string> paths)
        {
            bool error = false;
            /*if (paths == null)
                return false;
            foreach (var item in paths)
            {
                error = await LoadCmsItemFromDisk(item);
            }*/
            return error;
        }
    }


}
