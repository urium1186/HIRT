using System.CommandLine;

namespace HaloInfiniteResearchTools.Cli
{
    public class HirtRootCommand : RootCommand
    {
        public HirtRootCommand() : base()
        {
            this.AddCommand(new ListTagsOfCommand());
            this.AddCommand(new ExportRenderModelCommand());
            this.AddCommand(new ExportTextureCommand());
            this.AddCommand(new ExportJsonModelCommand());
        }


    }
}
