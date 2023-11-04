using HaloInfiniteResearchTools.Common;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    public class ExportFilesToJsonProcess : ProcessBase<List<string>>
    {
        private List<IHIRTFile> _files;
        private string _dir_path;
        private bool _advmode;
        private bool _outConsole;
        private List<string> _jsonFiles;

        public ExportFilesToJsonProcess(IHIRTFile file, string outfile, IServiceProvider? serviceProvider, bool advmode = false, bool outConsole = false) : base(serviceProvider)
        {
            _files = new List<IHIRTFile> { file };
            _dir_path = outfile;
            _advmode = advmode;
            _outConsole = outConsole;
            _jsonFiles = new List<string>();

        }
        public ExportFilesToJsonProcess(List<IHIRTFile> files, string dir_path)
        {
            _files = files;
            _dir_path = dir_path;
            _jsonFiles = new List<string>();
        }

        public override List<string> Result => _jsonFiles;

        protected override async Task OnExecuting()
        {
            if (_dir_path == null || _files == null || _files.Count == 0)
                return;

            Status = _files.Count > 1 ? "Opening Files" : "Opening File";
            UnitName = _files.Count > 1 ? "files opened" : "file opened";
            TotalUnits = _files.Count;
            IsIndeterminate = _files.Count == 1;
            _jsonFiles.Clear();
            var objLock = new object();
            Parallel.ForEach(_files, async filePath =>
            {
                var fileName = filePath.Name;

                try
                {
                    if (filePath.TagGroup != "����")
                    {
                        var temp_file = filePath ;
                        string dir_path = _dir_path + "\\" + temp_file.TagGroup + "\\";
                        string path_file = dir_path + Mmr3HashLTU.getMmr3HashFromInt(temp_file.TryGetGlobalId()) + (_advmode ? "_ADV.json" : ".json");
                        if (!Directory.Exists(dir_path))
                            Directory.CreateDirectory(dir_path);
                        if (!File.Exists(path_file))
                        {
                            var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve };

                            string jstonToWrite = "";
                            if (!_advmode)
                                jstonToWrite = (filePath as SSpaceFile).Deserialized()?.Root?.ToJson();
                            else
                            {
                                var process_js = new ReadTagInstanceProcessV2((LibHIRT.Files.Base.IHIRTFile)filePath);
                                await process_js.Execute();
                                if (process_js.TagParse.RootTagInst != null)
                                {
                                    jstonToWrite = JsonSerializer.Serialize(process_js.TagParse.RootTagInst, options);

                                }

                            }


                            if (!string.IsNullOrEmpty(jstonToWrite))
                            {
                                _jsonFiles.Add(jstonToWrite);
                                if (_outConsole)
                                    Console.WriteLine(jstonToWrite);
                                File.WriteAllText(path_file, jstonToWrite);
                            }

                        }

                    }

                }
                catch (Exception ex)
                {
                    StatusList.AddError(fileName, ex);
                }
                finally
                {
                    lock (objLock)
                    {
                        CompletedUnits++;
                    }

                }
            });
        }
    }
}
