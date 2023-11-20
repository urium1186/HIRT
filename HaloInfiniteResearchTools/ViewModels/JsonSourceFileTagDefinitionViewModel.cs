using HaloInfiniteResearchTools.Common;
using LibHIRT.Common;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpDX.Direct3D11;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(JsonSourceFileTagDefinitionFile))]
    public class JsonSourceFileTagDefinitionViewModel : SSpaceFileViewModel<JsonSourceFileTagDefinitionFile>
    {
        public string JsonString { get; set; }
        public JsonSourceFileTagDefinitionViewModel(IServiceProvider serviceProvider, JsonSourceFileTagDefinitionFile file) : base(serviceProvider, file)
        {
        }

        protected override Task OnInitializing()
        {
            if (File is SSpaceFile)
            {
                SSpaceFile temp = (SSpaceFile)File;
                var root = temp.Deserialized()?.Root;
                if (root != null)
                {
                    TagData data = root["schemaFileData"] as TagData;
                    if (data != null && data.ByteLengthCount != 0)
                    {
                        //JsonString = System.Text.Encoding.Default.GetString(data.ReadBuffer());
                        //JsonString =  data.ReadBuffer().ReadStringNullTerminated(0);
                        //JsonString =JsonString.Replace("/", "");
                        JsonString = "";
                        var obj= JObject.Parse(Encoding.UTF8.GetString(data.ReadBuffer()));
                        JsonString = obj.ToString();
                        /*
                        string jsonString = Encoding.UTF8.GetString(data.ReadBuffer());
                        using (JsonDocument document = JsonDocument.Parse(jsonString))
                        {
                            JsonString = document.ToString();
                        }
                        */
                    }
                }
            }
            return base.OnInitializing();
        }
    }
}
