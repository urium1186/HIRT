
using HaloInfiniteResearchTools.Common.Enumerations;
using HaloInfiniteResearchTools.Processes;
using LibHIRT.Files.FileTypes;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

namespace HaloInfiniteResearchTools.Cli
{
    public class ExportTextureCommand : Command
    {
        private readonly Dictionary<string, TextureFileFormat> validExte;
        private string _type_tag;
        private string _infile;
        private DirectoryInfo? _outfile;
        private string _ext;

        public ExportTextureCommand() : base("texture", "Operations on a texture")
        {
            validExte = new Dictionary<string, TextureFileFormat>();
            validExte["JPEG"] = TextureFileFormat.JPEG;
            validExte["QOI"] = TextureFileFormat.QOI;
            validExte["DDS"] = TextureFileFormat.DDS;
            validExte["TGA"] = TextureFileFormat.TGA;
            validExte["EXR"] = TextureFileFormat.EXR;
            validExte["PNG"] = TextureFileFormat.PNG;
            AddCommand(
                new Command("export", "Export to tga texture")
                    {
                        new Option<DirectoryInfo>(new string[] { "--deploy", "-d" }, "Deploy dir"),
                        new Option<string>(new string[] { "--text_id", "-ti" }, "Texture id"),
                        new Option<DirectoryInfo?>(new string[] { "--output", "-o" }, "Output dir path"),
                        new Option<string?>(new string[] { "--extension", "-e" }, "Output extension ( DDS, TGA, JPEG, PNG, EXR, QOI)"),
                    }
                .Also(cmd => cmd.SetHandler(
                    (DirectoryInfo deploy_dir, string infile, DirectoryInfo? outfile, string ext, InvocationContext ctx) => ExportToHandler(deploy_dir, infile, outfile, ext, true, ctx),
                    cmd.Options[0],
                    cmd.Options[1],
                    cmd.Options[2],
                    cmd.Options[3])));
        }

        private async void ExportToHandler(DirectoryInfo deploy_dir, string infile, DirectoryInfo? outfile, string ext, bool v, InvocationContext ctx)
        {
            _infile = infile;
            _outfile = outfile;
            _ext = ext;

            if (!validExte.ContainsKey(_ext))
            {
                Console.WriteLine("no soupurted extension.");
                return;
            }
            int id = int.Parse(infile);
            var process = new SearchFileByIdProcess(EntryPoint.ServiceProvider, id, true, deploy_dir.FullName);
            process.Completed += OpenFilesProcessExport_Completed;
            await process.Execute();
            Console.WriteLine("Tags listed to");
            /*var process = new OpenFilesProcess(EntryPoint.ServiceProvider, deploy_dir.FullName);
            process.Completed += OpenFilesProcessExport_Completed;
            await process.Execute();
            Console.WriteLine("Tags listed to");*/

        }

        private async void OpenFilesProcessExport_Completed(object? sender, EventArgs e)
        {
            try
            {
                //var founds = IHIFileContext.Instance.GetFiles<PictureFile>(_infile);
                var founds = ((SearchFileByIdProcess)sender).Result;
                if (founds != null && founds.Count() != 0)
                {

                    Models.TextureExportOptionsModel textureOptions = new Models.TextureExportOptionsModel();
                    textureOptions.ExportAllMips = false;
                    textureOptions.OutputNormalMapFormat = Common.Enumerations.NormalMapFormat.DirectX;
                    textureOptions.OutputPath = _outfile.FullName;
                    textureOptions.OutputFileFormat = validExte[_ext];
                    textureOptions.ExportTextureDefinition = false;
                    textureOptions.OverwriteExisting = true;
                    textureOptions.RecalculateNormalMapZChannel = false;


                    foreach (var item in founds)
                    {

                        if (item is PictureFile)
                        {
                            var exportProcess = new ExportTextureProcess((PictureFile)item, textureOptions);
                            //var exportProcess = new ExportModelProcess(_file, _sceneHelixToolkit, _renderModelDef, modelOptions, textureOptions, _nodes);
                            await exportProcess.Execute();
                        }

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
