
using HaloInfiniteResearchTools.Processes;
using HelixToolkit;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using Microsoft.Extensions.DependencyInjection;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace HaloInfiniteResearchTools.Cli
{
    public class ExportJsonModelCommand : Command
    {
        private string _type_tag;
        private string _infile;
        private FileInfo? _outfile;

        public ExportJsonModelCommand() : base("json", "Operations on a json")
        {

            AddCommand(
               new Command("export", "Export a tag to a json")
                   {
                        new Option<DirectoryInfo>(new string[] { "--deploy", "-d" }, "Deploy dir"),
                        new Option<string>(new string[] { "--tag_name", "-mn" }, "tag file name"),
                        new Option<FileInfo?>(new string[] { "--output", "-o" }, "Output file name"),
                   }
               .Also(cmd => cmd.SetHandler(
                   (DirectoryInfo deploy_dir, string infile, FileInfo? outfile, InvocationContext ctx) => ExportToJsonHandler(deploy_dir, infile, outfile, true, ctx),
                   cmd.Options[0],
                   cmd.Options[1],
                   cmd.Options[2])));
        }

        private async void ExportToJsonHandler(DirectoryInfo deploy_dir, string infile, FileInfo? outfile, bool v, InvocationContext ctx)
        {
            int index = infile.IndexOf('[');
            if (index != -1)
            {
                string truncatedString = infile.Substring(0, index);
                _infile = truncatedString;
            }
            else
            {
                _infile = infile;
            }
            _outfile = outfile;
            var process = new OpenFilesProcess(EntryPoint.ServiceProvider, deploy_dir.FullName);
            process.Completed += OpenFilesProcessExport_Completed;
            await process.Execute();
            Console.WriteLine("Tags listed to");
        }

        private async void OpenFilesProcessExport_Completed(object? sender, EventArgs e)
        {
            var founds = EntryPoint.ServiceProvider.GetRequiredService<IHIFileContext>().GetFiles<GenericFile>(_infile);
            //Console.WriteLine(founds.ToList().EnumerateToString());
            foreach (var item in founds)
            {
                var exportProcess = new ExportFilesToJsonProcess(item, EntryPoint.ServiceProvider);
                Console.WriteLine(item.Name);
                await exportProcess.Execute();
            }
        }
    }
}
