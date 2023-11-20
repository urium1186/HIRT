using HaloInfiniteResearchTools.Common;
using HavokScriptToolsCommon;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(HsSourceFile))]
    public class HsSourceViewModel : SSpaceFileViewModel<HsSourceFile>
    {
        public string HsSource { get; set; }
        public HsSourceViewModel(IServiceProvider serviceProvider, HsSourceFile file) : base(serviceProvider, file)
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
                    TagData data = root["data"] as TagData;
                    if (data != null && data.ByteLengthCount != 0)
                    {
                        HsSource = System.Text.Encoding.Default.GetString(data.ReadBuffer());
                        
                    }
                }
            }
            return base.OnInitializing();
        }
    }
}
