using HaloInfiniteResearchTools.Models;
using LibHIRT.TagReader.Dumper;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    public class TagStructsDumperProcess : ProcessBase
    {
        TagStructsDumperOptionsModel optionsModel;
        private TagStructsDumper? structsDumper = null;

        public TagStructsDumperOptionsModel OptionsModel { get => optionsModel; }

        public TagStructsDumperProcess(TagStructsDumperOptionsModel optionsModel)
        {
            this.optionsModel = optionsModel;
        }
        protected override async Task OnInitializing()
        {
            if (structsDumper == null)
                structsDumper = new TagStructsDumper();
            structsDumper.OutDIR = optionsModel.OutputPath;
            structsDumper.StartAddress = optionsModel.LastStartAddress;
            structsDumper.GameLocation = optionsModel.GameLocation;

        }
        protected override async Task OnExecuting()
        {
            if (!string.IsNullOrEmpty( optionsModel.SearchTerm))
            {
                await structsDumper.SearchInMem(optionsModel.SearchTerm);
            }
            else {
                await structsDumper.Dump();
            }
            
            optionsModel.LastStartAddress = structsDumper.StartAddress;
        }

        public void SetStatus(string message)
        {

        }

    }
}
