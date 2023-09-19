
using Aspose.ThreeD.Shading;
using HaloInfiniteResearchTools.Common.Enumerations;
using HaloInfiniteResearchTools.Processes;
using LibHIRT.Exporters;
using LibHIRT.Files;
using LibHIRT.Processes.OnGeometry;
using LibHIRT.Serializers;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

namespace HaloInfiniteResearchTools.Cli
{
    public class ExportRenderGeometryOnTagInstanceCommand : Command
    {
        private readonly Dictionary<string, TextureFileFormat> validExte;
        private string _type_tag;
        private string _infile;
        private DirectoryInfo? _outfile;
        private bool _from_tp;
        private string _tg_path;
        private bool _tif;
        private List<RenderGeometryTag> rendersGeometrys;
        private List<Material> _materialList;
        private string _format;

        public ExportRenderGeometryOnTagInstanceCommand() : base("export_rg", "export render geometry (dae) in tag")
        {
            AddCommand(
               new Command("onpath", "Search in folder path")
                   {
                    new Option<DirectoryInfo>(new string[] { "--deploy", "-d" }, "Deploy dir"),
                    new Option<string>(new string[] { "--tag_id", "-ti" }, "TagInstance id"),
                    new Option<string>(new string[] { "--tag_id_format", "-tif" }, "true or false, if is int or hash"),
                    new Option<DirectoryInfo?>(new string[] { "--output", "-o" }, "Output dir path"),
                    new Option<string?>(new string[] { "--export_format", "-ef" }, "Format to export one of this ( dae, fbx), is not dae default"),
            }.Also(cmd => cmd.SetHandler(
                (DirectoryInfo deploy_dir, string infile, string tif, DirectoryInfo? outfile, string ext, InvocationContext ctx) => ExportToHandler(deploy_dir, infile, tif, outfile, ext, true, ctx),
                cmd.Options[0],
                cmd.Options[1],
                cmd.Options[2],
                cmd.Options[3],
                cmd.Options[4]
                )));

            AddCommand(
               new Command("onmodule", "Search in module")
                   {
                    new Option<FileInfo>(new string[] { "--deploy", "-d" }, "Module path"),
                    new Option<string>(new string[] { "--tag_id", "-ti" }, "TagInstance id"),
                    new Option<string>(new string[] { "--tag_id_format", "-tif" }, "true or false, if is tag id is int (true) or hash (false)"),
                    new Option<DirectoryInfo?>(new string[] { "--output", "-o" }, "Output dir path"),
                    new Option<string?>(new string[] { "--export_format", "-ef" }, "Format to export one of this ( dae, fbx), is not dae default"),
            }.Also(cmd => cmd.SetHandler(
                (FileInfo deploy_dir, string infile, string tif, DirectoryInfo? outfile, string format, InvocationContext ctx) => ExportToHandler(deploy_dir, infile, tif, outfile, format, true, ctx),
                cmd.Options[0],
                cmd.Options[1],
                cmd.Options[2],
                cmd.Options[3],
                cmd.Options[4]
                )));
        }

        private async void ExportToHandler(FileInfo deploy_dir, string infile, string tif, DirectoryInfo? outfile, string format,  bool v, InvocationContext ctx)
        {
            _infile = infile;
            _outfile = outfile;
            _format = format;
            
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
        private async void ExportToHandler(DirectoryInfo deploy_dir, string infile, string tif, DirectoryInfo? outfile, string format,bool v, InvocationContext ctx)
        {
            _infile = infile;
            _outfile = outfile;
            _format = format;
            
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

                        if (rendersGeometrys == null)
                            rendersGeometrys = new List<RenderGeometryTag>();
                        else
                            rendersGeometrys.Clear();
                        if (_materialList == null)
                            _materialList = new List<Material>();
                        else
                            _materialList.Clear();
                        var readProcess = new ReadTagInstanceProcess((LibHIRT.Files.Base.IHIRTFile)item);
                        readProcess.OnInstanceLoadEvent += ExportProcess_OnInstanceLoadEvent;

                        await readProcess.Execute();
                        int i = 0;
                        foreach (var item_rg in rendersGeometrys)
                        {
                            if (item_rg == null)
                                continue;
                            var renderGeometry = RenderGeometrySerializer.Deserialize(null, (SSpaceFile)item, item_rg);

                            if (true)
                            {
                                string filename = Path.GetFileNameWithoutExtension(item.Name) + (rendersGeometrys.Count > 1 ? "_" + i.ToString() : "");
                                RenderGeometryExporter.Export(renderGeometry, _outfile.FullName, filename, _materialList, _format);
                            }
                            else
                            {
                                var convertProcess = new ConvertRenderGeometryToAssimpSceneProcess(renderGeometry, ((SSpaceFile)item).FileMemDescriptor.GlobalTagId1.ToString("X"));
                                await convertProcess.Execute();
                                var exportProcess = new ExportModelProcess((SSpaceFile)item, convertProcess.Result, null, modelOptions, textureOptions, null);
                                await exportProcess.Execute();
                            }


                            i++;
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

        private void ExportProcess_OnInstanceLoadEvent(object? sender, ITagInstance e)
        {
            if (e is RenderGeometryTag)
            {
                if (rendersGeometrys == null)
                    rendersGeometrys = new List<RenderGeometryTag>();
                rendersGeometrys.Add((RenderGeometryTag)e);
            }
            if (e.TagDef.N == "materials" && e is Tagblock)
            {
                // initialize PBR material object
                using (var list = (e as Tagblock))
                {
                    int i = 1; 
                    foreach (var item in list)
                    {
                        PbrMaterial mat = new PbrMaterial();

                        mat.Name = (item["material"] as TagRef).Ref_id;

                        // an almost metal material

                        mat.MetallicFactor = 0.9;

                        // material surface is very rough

                        mat.RoughnessFactor = 0.9;

                        mat.Albedo = new Aspose.ThreeD.Utilities.Vector3(255/i, 255 / i%2, 0 );

                        _materialList.Add(mat);
                        i++;
                    }
                }

            }
        }
    }
}
