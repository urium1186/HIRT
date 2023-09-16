
using HaloInfiniteResearchTools.Processes;
using LibHIRT.Files;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

namespace HaloInfiniteResearchTools.Cli
{
    public class UnpackTagInstanceCommand : Command
    {
        private string _searchterm;
        private DirectoryInfo? _output;

        public UnpackTagInstanceCommand() : base("pakage", "unpack a file")
        {
            AddCommand(
               new Command("unpack", "Search in folder path and unpack")
                   {
                    new Option<DirectoryInfo>(new string[] { "--deploy", "-d" }, "Deploy dir"),
                    new Option<string>(new string[] { "--searchterm", "-st" }, "Search term"),
                    new Option<DirectoryInfo?>(new string[] { "--output", "-o" }, "Output dir path"),
            }.Also(cmd => cmd.SetHandler(
                (DirectoryInfo deploy_dir, string searchterm, DirectoryInfo output, InvocationContext ctx) => ExportToHandler(deploy_dir, searchterm, output, true, ctx),
                cmd.Options[0],
                cmd.Options[1],
                cmd.Options[2]
                )));
        }

        private async void ExportToHandler(DirectoryInfo deploy_dir, string searchterm, DirectoryInfo? output, bool v, InvocationContext ctx)
        {
            _searchterm = searchterm;
            _output = output;

            if (!deploy_dir.Exists)
            {
                Console.WriteLine("Must be a valid path");
                return;
            }

            var process = new OpenFilesProcess(EntryPoint.ServiceProvider, deploy_dir.FullName);
            process.Completed += OpenFilesProcessExport_Completed;
            await process.Execute();
            if (process.StatusList.Count != 0)
            {
                foreach (var item in process.StatusList)
                {
                    Console.WriteLine("Status info " + item);
                }
            }
            Console.WriteLine("Process completed");
        }

        private async void OpenFilesProcessExport_Completed(object? sender, EventArgs e)
        {
            try
            {
                var founds = EntryPoint.ServiceProvider.GetRequiredService<IHIFileContext>().GetFiles(_searchterm);
                if (founds != null && founds.Count() != 0)
                {

                    foreach (var item in founds)
                    {
                        string path = Path.Combine(_output.FullName, ((SSpaceFile)item).FileMemDescriptor.Path_string.Replace("����", "no_tag_group") + ".bin");
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        byte[] _out = new byte[item.GetStream().Length];
                        item.GetStream().Seek(0, SeekOrigin.Begin);
                        item.GetStream().Read(_out);
                        File.WriteAllBytes(path, _out);
                    }


                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error: " + ex.Message);
            }


            Console.WriteLine("Termino el proceso");
        }
    }
}
