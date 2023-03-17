using HaloInfiniteResearchTools.Models;
using LibHIRT.TagReader.Dumper;
using Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

namespace HaloInfiniteResearchTools.Processes
{
    public class TagStructsDumperProcess : ProcessBase
    {
        TagStructsDumperOptionsModel optionsModel;
        private TagStructsDumper? structsDumper=null;

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

        }
        protected override async Task OnExecuting()
        {
            await structsDumper.Dump();
            optionsModel.LastStartAddress= structsDumper.StartAddress;
        }

        public void SetStatus(string message)
        {
            
        }

    }
}
