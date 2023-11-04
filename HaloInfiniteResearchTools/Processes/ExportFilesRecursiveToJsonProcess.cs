using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    class ExportFilesRecursiveToJsonProcess : ProcessBase<IEnumerable<string>>
    {
        private IHIRTFile _file;
        private string _dir_path;
        private ConcurrentDictionary<int, IHIRTFile> _files;

        public ExportFilesRecursiveToJsonProcess(IHIRTFile file, string dir_path)
        {
            _file = file;
            _dir_path = dir_path;

        }

        public override IEnumerable<string> Result => throw new NotImplementedException();

        protected override Task OnInitializing()
        {
            if (_files == null)
                _files = new ConcurrentDictionary<int, IHIRTFile>();
            _files.Clear();
            return base.OnInitializing();
        }
        protected override async Task OnExecuting()
        {
            if (_dir_path == null || _file == null)
                return;

            Status = "Exporting Files";
            UnitName = "files opened";
            TotalUnits = -1;
            IsIndeterminate = true;

            var objLock = new object();

            var fileName = _file.Name;

            try
            {
                if (_file.TagGroup != "����")
                {
                    var temp_file = _file;
                    string dir_path = _dir_path + "\\" + temp_file.TagGroup + "\\";
                    string path_file = dir_path + Mmr3HashLTU.getMmr3HashFromInt(temp_file.TryGetGlobalId()) + ".json";
                    if (!Directory.Exists(dir_path))
                        Directory.CreateDirectory(dir_path);

                    if (!File.Exists(path_file))
                    {
                        await ExportFileToJson(_file,path_file);

                    }

                }

                /* if (!_fileContext.OpenFile(filePath))
                 {

                     StatusList.AddWarning(fileName, "Failed to open file.");
                 }
                 else
                 {
                     Status = Status.Replace("\n" + temp, "");
                     _filesLoaded.Add(fileName);
                     StatusList.AddMessage(fileName, "Open file.");
                 }*/

            }
            catch (Exception ex)
            {
                StatusList.AddError(fileName, ex);
            }
            finally
            {
                lock (objLock)
                {
                    //CompletedUnits++;
                }

            }
        }

        private async Task ExportFileToJson(IHIRTFile file, string path_file)
        {
            if (_files.ContainsKey(file.TryGetGlobalId()))
            {
                return;
            }
            if (File.Exists(path_file))
                return;
            var process = new ReadTagInstanceProcessV2(file);

            process.OnInstanceLoadEvent += TagParse_OnInstanceLoadEvent;
            if (!_files.TryAdd(file.TryGetGlobalId(), file))
                return;

            await Task.Factory.StartNew(process.Execute, TaskCreationOptions.LongRunning);
            await process.CompletionTask;
            string jstonToWrite = process.TagParse?.RootTagInst?.ToJson();
            if (!string.IsNullOrEmpty(jstonToWrite))
                if (!File.Exists(path_file))
                    try
                    {
                        File.WriteAllText(path_file, jstonToWrite);
                    }
                    catch (Exception ex)
                    {

                    }

        }

        private async void TagParse_OnInstanceLoadEvent(object? sender, ITagInstance e)
        {

            if (e != null)
            {
                if (e is TagRef)
                {
                    var tagRef = e as TagRef;
                    if (tagRef.TagGroupRev != "����")
                    {

                        string dir_path = _dir_path + "\\" + tagRef.TagGroupRev + "\\";
                        string path_file = dir_path + tagRef.Ref_id + ".json";
                        if (!Directory.Exists(dir_path))
                            Directory.CreateDirectory(dir_path);

                        if (!File.Exists(path_file))
                        {
                            ISSpaceFile file = null;
                            file = HIFileContext.Instance.GetFile((int)tagRef.Ref_id_int);
                                if (file == null)
                                {
                                    file = HIFileContext.Instance.GetFile((int)tagRef.Ref_id_center_int);
                                    if (file == null)
                                    {
                                        file = HIFileContext.Instance.GetFile((int)tagRef.Ref_id_sub_int);
                                    }

                                }

                            
                            if (file != null)
                            {
                                await ExportFileToJson((SSpaceFile)file, path_file);
                            }
                        }
                    }
                }
            }
        }
    }
}
