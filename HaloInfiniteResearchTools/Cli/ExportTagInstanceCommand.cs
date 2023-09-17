
using HaloInfiniteResearchTools.Common.Enumerations;
using HaloInfiniteResearchTools.Processes;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

namespace HaloInfiniteResearchTools.Cli
{
    public class ExportTagInstance : Command
    {
        private readonly Dictionary<string, TextureFileFormat> validExte;
        private string _type_tag;
        private string _infile;
        private DirectoryInfo? _outfile;
        private bool _ext;
        private bool _consout;
        private bool _tif;

        public ExportTagInstance() : base("export", "export a json")
        {
            AddCommand(
               new Command("onpath", "Search in folder path")
                   {
                    new Option<DirectoryInfo>(new string[] { "--deploy", "-d" }, "Deploy dir"),
                    new Option<string>(new string[] { "--tag_id", "-ti" }, "TagInstance id"),
                    new Option<string>(new string[] { "--tag_id_format", "-tif" }, "true or false, if is int or hash"),
                    new Option<DirectoryInfo?>(new string[] { "--output", "-o" }, "Output dir path"),
                    new Option<string?>(new string[] { "--jsonplus", "-jp" }, "true or false, create a ligt json or complex"),
                    new Option<string?>(new string[] { "--consoleout", "-co" }, "true or false, print to console out put"),
            }.Also(cmd => cmd.SetHandler(
                (DirectoryInfo deploy_dir, string infile, string tif, DirectoryInfo? outfile, string ext, string consout, InvocationContext ctx) => ExportToHandler(deploy_dir, infile, tif, outfile, ext, consout, true, ctx),
                cmd.Options[0],
                cmd.Options[1],
                cmd.Options[2],
                cmd.Options[3],
                cmd.Options[4],
                cmd.Options[5]
                )));

            AddCommand(
               new Command("onmodule", "Search in module")
                   {
                    new Option<FileInfo>(new string[] { "--deploy", "-d" }, "Module path"),
                    new Option<string>(new string[] { "--tag_id", "-ti" }, "TagInstance id"),
                    new Option<string>(new string[] { "--tag_id_format", "-tif" }, "true or false, if is tag id is int (true) or hash (false)"),
                    new Option<DirectoryInfo?>(new string[] { "--output", "-o" }, "Output dir path"),
                    new Option<string?>(new string[] { "--jsonplus", "-jp" }, "true or false, create a ligt json or complex"),
                    new Option<string?>(new string[] { "--consoleout", "-co" }, "true or false, print to console out put"),
            }.Also(cmd => cmd.SetHandler(
                (FileInfo deploy_dir, string infile, string tif, DirectoryInfo? outfile, string ext, string consout, InvocationContext ctx) => ExportToHandler(deploy_dir, infile, tif, outfile, ext, consout, true, ctx),
                cmd.Options[0],
                cmd.Options[1],
                cmd.Options[2],
                cmd.Options[3],
                cmd.Options[4],
                cmd.Options[5]
                )));
        }

        private async void ExportToHandler(FileInfo deploy_dir, string infile, string tif, DirectoryInfo? outfile, string ext, string consout, bool v, InvocationContext ctx)
        {
            _infile = infile;
            _outfile = outfile;
            _ext = bool.Parse(ext);
            _consout = bool.Parse(consout);
            _tif = bool.Parse(tif);

            if (!deploy_dir.Exists || deploy_dir.Extension != ".module")
            {
                Console.WriteLine("Must be a module file");
                return;
            }
            int id = _tif ? int.Parse(infile) : Mmr3HashLTU.fromStrHash(infile);
            var process = new SearchFileByIdProcess(EntryPoint.ServiceProvider, id, false, deploy_dir.FullName);
            process.Completed += OpenFilesProcessExport_Completed;
            await process.Execute();
            Console.WriteLine("Tags listed to");
        }
        private async void ExportToHandler(DirectoryInfo deploy_dir, string infile, string tif, DirectoryInfo? outfile, string ext, string consout, bool v, InvocationContext ctx)
        {
            _infile = infile;
            _outfile = outfile;
            _ext = bool.Parse(ext);
            _consout = bool.Parse(consout);
            _tif = bool.Parse(tif);
            int id = _tif ? int.Parse(infile) : Mmr3HashLTU.fromStrHash(infile);
            var process = new SearchFileByIdProcess(EntryPoint.ServiceProvider, id, false, deploy_dir.FullName);
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

                    foreach (var item in founds)
                    {
                        var exportProcess = new ExportFilesToJsonProcess(item, _outfile.FullName, EntryPoint.ServiceProvider, _ext, _consout);

                        await exportProcess.Execute();
                        //                        Console.WriteLine(exportProcess.Result);
                        /*
                        var process = new ReadTagInstanceProcess((LibHIRT.Files.Base.IHIRTFile)item);
                        await process.Execute();
                        if (process.TagParse.RootTagInst != null) {
                            string jsonString = JsonSerializer.Serialize(process.TagParse.RootTagInst, options);
                            Console.WriteLine(jsonString);
                        }
                        */

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
