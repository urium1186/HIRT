using System;
using System.CommandLine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine.Invocation;
using System.IO;

namespace HaloInfiniteResearchTools.Cli
{
    public class HirtRootCommand : RootCommand
    {
        public HirtRootCommand() : base()
        {
            this.AddCommand(new ListTagsOfCommand());
            this.AddCommand(new ExportRenderModelCommand());
            this.AddCommand(new ExportTextureCommand());
        }

       
    }
}
