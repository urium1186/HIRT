
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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace HaloInfiniteResearchTools.Cli
{
    public class ExportRenderModelCommand : Command
    {
        private string _type_tag;
        private string _infile;
        private FileInfo? _outfile;

        public ExportRenderModelCommand() : base("model", "Operations on a render model")
        {
            
             AddCommand(
                new Command("export", "Export to fbx a render model tag")
                    {
                        new Option<DirectoryInfo>(new string[] { "--deploy", "-d" }, "Deploy dir"),
                        new Option<string>(new string[] { "--mode_name", "-mn" }, "Mode file name"),
                        new Option<FileInfo?>(new string[] { "--output", "-o" }, "Output file name"),
                    }
                .Also(cmd => cmd.SetHandler(
                    (DirectoryInfo deploy_dir, string infile, FileInfo? outfile, InvocationContext ctx) => ExportToFbxHandler(deploy_dir, infile, outfile, true, ctx),
                    cmd.Options[0],
                    cmd.Options[1],
                    cmd.Options[2])));
        }

        private async void ExportToFbxHandler(DirectoryInfo deploy_dir, string infile, FileInfo? outfile, bool v, InvocationContext ctx)
        {
            _infile = infile;
            _outfile = outfile;

            int id = int.Parse(infile);
            var process = new SearchFileByIdProcess(EntryPoint.ServiceProvider, id, deploy_dir.FullName);
            process.Completed += OpenFilesProcessExport_Completed;
            await process.Execute();
            Console.WriteLine("Tags listed to");
            /*
            var process = new OpenFilesProcess(EntryPoint.ServiceProvider, deploy_dir.FullName);
            process.Completed += OpenFilesProcessExport_Completed;
            await process.Execute();
            Console.WriteLine("Tags listed to");*/
        }

        private async void OpenFilesProcessExport_Completed(object? sender, EventArgs e)
        {
            //var founds = EntryPoint.ServiceProvider.GetRequiredService<IHIFileContext>().GetFiles<RenderModelFile>(_infile);
            var founds = ((SearchFileByIdProcess)sender).Result;
            if (founds != null && founds.Count() != 0) {
                Models.ModelExportOptionsModel modelOptions = new Models.ModelExportOptionsModel();
                Models.TextureExportOptionsModel textureOptions = new Models.TextureExportOptionsModel();
                modelOptions.OutputFileFormat = Common.Enumerations.ModelFileFormat.FBX;
                modelOptions.CreateDirectoryForModel = true;
                modelOptions.ExportTextures = false;
                modelOptions.ExportMaterialDefinitions = false;
                modelOptions.OutputPath = _outfile.FullName;
                modelOptions.OverwriteExisting = true;
                foreach (var item in founds)
                {
                    if (item is RenderModelFile) {
                        var exportProcess = new ExportModelProcess(item, modelOptions, textureOptions, EntryPoint.ServiceProvider);
                        //var exportProcess = new ExportModelProcess(_file, _sceneHelixToolkit, _renderModelDef, modelOptions, textureOptions, _nodes);
                        await exportProcess.Execute();
                    }
                }
            }

            Console.WriteLine("Termino el proceso");
        }
    }
}
