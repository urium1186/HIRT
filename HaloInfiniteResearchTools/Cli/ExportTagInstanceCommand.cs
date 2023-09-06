
using HaloInfiniteResearchTools.Processes;
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
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Shapes;
using HaloInfiniteResearchTools.Common.Enumerations;
using System.Xml.Linq;
using static System.Windows.Forms.Design.AxImporter;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaloInfiniteResearchTools.Cli
{
    public class ExportTagInstance : Command
    {
        private readonly Dictionary<string, TextureFileFormat> validExte;
        private string _type_tag;
        private string _infile;
        private DirectoryInfo? _outfile;
        private string _ext;

        public ExportTagInstance() : base("export", "export a json")
        {
            AddCommand(
               new Command("onpath", "Search in folder path")
                   {
                    new Option<DirectoryInfo>(new string[] { "--deploy", "-d" }, "Deploy dir"),
                    new Option<string>(new string[] { "--tag_id", "-ti" }, "TagInstance id"),
                    new Option<DirectoryInfo?>(new string[] { "--output", "-o" }, "Output dir path"),
                    new Option<string?>(new string[] { "--putintopath", "-pi" }, "True or Folse, create a dir from tag path"),
            }.Also(cmd => cmd.SetHandler(
                (DirectoryInfo deploy_dir, string infile, DirectoryInfo? outfile, string ext ,InvocationContext ctx) => ExportToHandler(deploy_dir, infile, outfile, ext, true, ctx),
                cmd.Options[0],
                cmd.Options[1],
                cmd.Options[2],
                cmd.Options[3])));

            AddCommand(
               new Command("onmodule", "Search in module")
                   {
                    new Option<FileInfo>(new string[] { "--deploy", "-d" }, "Module path"),
                    new Option<string>(new string[] { "--tag_id", "-ti" }, "TagInstance id"),
                    new Option<DirectoryInfo?>(new string[] { "--output", "-o" }, "Output dir path"),
                    new Option<string?>(new string[] { "--putintopath", "-pi" }, "True or Folse, create a dir from tag path"),
            }.Also(cmd => cmd.SetHandler(
                (FileInfo deploy_dir, string infile, DirectoryInfo? outfile, string ext, InvocationContext ctx) => ExportToHandler(deploy_dir, infile, outfile, ext, true, ctx),
                cmd.Options[0],
                cmd.Options[1],
                cmd.Options[2],
                cmd.Options[3])));
        }

        private async void ExportToHandler(FileInfo deploy_dir, string infile, DirectoryInfo? outfile, string ext ,bool v, InvocationContext ctx)
        {
            _infile = infile;
            _outfile = outfile;
            _ext = ext;

            if (!deploy_dir.Exists || deploy_dir.Extension != ".module") {
                Console.WriteLine("Must be a module file");
                return;
            }
           
            var process = new SearchFileByIdProcess(EntryPoint.ServiceProvider, int.Parse(infile), deploy_dir.FullName);
            process.Completed += OpenFilesProcessExport_Completed;
            await process.Execute();
            Console.WriteLine("Tags listed to");
        }
        private async void ExportToHandler(DirectoryInfo deploy_dir, string infile, DirectoryInfo? outfile, string ext ,bool v, InvocationContext ctx)
        {
            _infile = infile;
            _outfile = outfile;
            _ext = ext;
           
            
                
            var process = new SearchFileByIdProcess(EntryPoint.ServiceProvider, int.Parse(infile), deploy_dir.FullName);
            process.Completed += OpenFilesProcessExport_Completed;
            await process.Execute();
            Console.WriteLine("Tags listed to");
        }

        private async void OpenFilesProcessExport_Completed(object? sender, EventArgs e)
        {
            try
            {
                var founds = ((SearchFileByIdProcess)sender).Result;
                if (founds != null && founds.Count() != 0)
                {
                    var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve };
                    foreach (var item in founds)
                    {
                        var process = new ReadTagInstanceProcess((LibHIRT.Files.Base.IHIRTFile)item);
                        await process.Execute();
                        if (process.TagParse.RootTagInst != null) {
                            string jsonString = JsonSerializer.Serialize(process.TagParse.RootTagInst, options);
                            Console.WriteLine(jsonString);
                        }
                        
                    }


                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error: "+ ex.Message);
            }
            

            Console.WriteLine("Termino el proceso");
        }
    }
}
