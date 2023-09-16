
using HaloInfiniteResearchTools.Processes;
using LibHIRT.Files;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;

namespace HaloInfiniteResearchTools.Cli
{
    public class ListTagsOfCommand : Command
    {
        private string _type_tag;
        private string _infile;
        private FileInfo? _outfile;

        public ListTagsOfCommand() : base("tags", "List/Pack/unpack a tag file")
        {
            AddCommand(
                new Command("list", "List tag files")
                    {
                        new Option<DirectoryInfo>(new string[] { "--deploy", "-d" }, "Deploy dir"),
                        new Option<string>(new string[] { "--type", "-t" }, "Output file name"),
                        new Option<FileInfo?>(new string[] { "--output", "-o" }, "Output file name"),
                    }
                .Also(cmd => cmd.SetHandler(
                    (DirectoryInfo deploy_dir, string infile, FileInfo? outfile, InvocationContext ctx) => ListTagOsHandler(deploy_dir, infile, outfile, true, ctx),
                    cmd.Options[0],
                    cmd.Options[1],
                    cmd.Options[2])));
            AddCommand(new ExportTagInstance());
            AddCommand(new ExportRenderGeometryOnTagInstanceCommand());
            AddCommand(new ExportSbpsCommand());
            AddCommand(new DebugTagCommand());
            AddCommand(new UnpackTagInstanceCommand());
            /*
            AddCommand(
                new Command("unpack", "Unpack a bond file")
                    {
                        new Argument<FileInfo>("filename", "Bond file to unpack"),
                        new Option<FileInfo?>(new string[] { "--output", "-o" }, "Output file name"),
                    }
                .Also(cmd => cmd.SetHandler(
                    (FileInfo infile, FileInfo? outfile, InvocationContext ctx) => CacheFileUnpackHandler(infile, outfile, true, ctx),
                    cmd.Arguments[0],
                    cmd.Options[0])));
            AddCommand(
               new Command("pack", "Pack a bond file")
                    {
                        new Argument<FileInfo>("filename", "Bond file to pack"),
                        new Option<FileInfo?>(new string[] { "--output", "-o" }, "Output file name")
                    }
                    .Also(cmd => cmd.SetHandler(
                        (FileInfo infile, FileInfo? outfile, InvocationContext ctx) => CacheFilePackHandler(infile, outfile, ctx),
                        cmd.Arguments[0],
                        cmd.Options[0])));
            */

        }

        private async void ListTagOsHandler(DirectoryInfo deploy_dir, string type_tag, FileInfo? outfile, bool v, InvocationContext ctx)
        {
            _type_tag = type_tag;
            _outfile = outfile;
            var process = new OpenFilesProcess(EntryPoint.ServiceProvider, deploy_dir.FullName);
            process.Completed += OpenFilesProcess_Completed;
            await process.Execute();
            Console.WriteLine("Tags listed to");
        }

        private void OpenFilesProcess_Completed(object? sender, EventArgs e)
        {
            var founds = EntryPoint.ServiceProvider.GetRequiredService<IHIFileContext>().GetFiles("." + _type_tag);
            StringBuilder outPutPath = new StringBuilder();
            foreach (var file in founds)
            {
                outPutPath.AppendLine(file.Path_string);
            }
            //FileStream fileStream= new FileStream(_outfile.FullName,FileMode.OpenOrCreate, FileAccess.ReadWrite);
            File.WriteAllText(_outfile.FullName, outPutPath.ToString());

            Console.WriteLine(outPutPath);
            Console.WriteLine("Tags listed to " + founds.Count().ToString());

        }
    }
}
