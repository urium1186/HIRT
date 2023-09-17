
using Aspose.ThreeD.Shading;
using HaloInfiniteResearchTools.Assimport;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.Processes.OnGeometry;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

namespace HaloInfiniteResearchTools.Cli
{
    public class ExportSbpsCommand : Command
    {
        private string _infile;
        private DirectoryInfo? _outfile;
        private bool _tif;
        private List<RenderGeometryTag> rendersGeometrys;
        private List<Material> _materialList;

        public ExportSbpsCommand() : base("export_sbps", "export sbps (fbx) in file")
        {
            AddCommand(
               new Command("onpath", "Search in folder path")
                   {
                    new Option<DirectoryInfo>(new string[] { "--deploy", "-d" }, "Deploy dir"),
                    new Option<string>(new string[] { "--tag_id", "-ti" }, "sbps id"),
                    new Option<string>(new string[] { "--tag_id_format", "-tif" }, "true or false, if is int or hash"),
                    new Option<DirectoryInfo?>(new string[] { "--output", "-o" }, "Output dir path"),
            }.Also(cmd => cmd.SetHandler(
                (DirectoryInfo deploy_dir, string infile, string tif, DirectoryInfo? outfile, InvocationContext ctx) => ExportToHandler(deploy_dir, infile, tif, outfile, true, ctx),
                cmd.Options[0],
                cmd.Options[1],
                cmd.Options[2],
                cmd.Options[3]
                )));
        }

        private async void ExportToHandler(DirectoryInfo deploy_dir, string infile, string tif, DirectoryInfo? outfile, bool v, InvocationContext ctx)
        {
            _infile = infile;
            _outfile = outfile;
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
                if (founds != null && founds.Count() == 1 && founds.First() is ScenarioStructureBspFile)
                {

                    Models.ModelExportOptionsModel modelOptions = new Models.ModelExportOptionsModel();
                    Models.TextureExportOptionsModel textureOptions = new Models.TextureExportOptionsModel();
                    modelOptions.OutputFileFormat = Common.Enumerations.ModelFileFormat.FBX;
                    modelOptions.CreateDirectoryForModel = true;
                    modelOptions.ExportTextures = false;
                    modelOptions.ExportMaterialDefinitions = false;
                    modelOptions.OutputPath = _outfile.FullName;
                    modelOptions.OverwriteExisting = true;

                    var item = (ScenarioStructureBspFile)founds.First();
                    {
                        if (_materialList == null)
                            _materialList = new List<Material>();
                        else
                            _materialList.Clear();
                        var _context = new HISceneContext(item.Name);
                        var readProcess = new LoadSbpsToContextProcess(_context, item, item.FileMemDescriptor.GlobalTagId1.ToString("X"));
                        await readProcess.Execute();
                        foreach (var message in readProcess.StatusList)
                        {
                            Console.WriteLine(message.Message);
                        }

                        var exportProcess = new ExportModelProcess((SSpaceFile)item, _context.Scene, null, modelOptions, textureOptions, null);
                        await exportProcess.Execute();
                        foreach (var message in exportProcess.StatusList)
                        {
                            Console.WriteLine(message.Message);
                        }

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
