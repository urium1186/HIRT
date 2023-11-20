using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using HavokScriptToolsCommon;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(LuaScriptTagDefinitionFile))]
    public class LuaScriptTagDefinitionViewModel : SSpaceFileViewModel<LuaScriptTagDefinitionFile>, IDisposeWithView
    {
        public string LuaCode { get; set; }
        public LuaScriptTagDefinitionViewModel(IServiceProvider serviceProvider, LuaScriptTagDefinitionFile file) : base(serviceProvider, file)
        {
        }

        protected override Task OnInitializing()
        {
            if (File is SSpaceFile) { 
                SSpaceFile temp = (SSpaceFile)File;
                var root = temp.Deserialized()?.Root;
                if (root != null)
                {
                    TagData data = root["luaFileData"] as TagData;
                    if (data != null && data.ByteLengthCount!=0)
                    {
                        HksDisassembler hksDisassembler = new HksDisassembler(data.ReadBuffer());
                        LuaCode = hksDisassembler.Disassemble();
                    }
                }
            }
            return base.OnInitializing();
        }
    }
}
