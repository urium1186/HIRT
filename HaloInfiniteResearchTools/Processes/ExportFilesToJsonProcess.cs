using LibHIRT.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    class ExportFilesToJsonProcess : ProcessBase<IEnumerable<string>>
    {
        private List<ISSpaceFile> _files;
        private string _dir_path;

        public ExportFilesToJsonProcess(List<ISSpaceFile> files, string dir_path)
        {
            _files = files;
            _dir_path = dir_path;
        }

        public override IEnumerable<string> Result => throw new NotImplementedException();

        protected override async Task OnExecuting()
        {
            if (_dir_path == null || _files == null || _files.Count == 0)
                return;

            Status = _files.Count > 1 ? "Opening Files" : "Opening File";
            UnitName = _files.Count > 1 ? "files opened" : "file opened";
            TotalUnits = _files.Count;
            IsIndeterminate = _files.Count == 1;

            var objLock = new object();
            _ = Parallel.ForEach(_files, filePath =>
            {
                var fileName = filePath.Name;

                try
                {
                    if (filePath.TagGroup != "����")
                    {
                        var temp_file = filePath as SSpaceFile;
                        string dir_path = _dir_path + "\\";
                        string path_file = dir_path + temp_file.FileMemDescriptor.GlobalTagId1 + "_" + temp_file.TagGroup + ".json";
                        if (!Directory.Exists(dir_path))
                            Directory.CreateDirectory(dir_path);
                        if (!File.Exists(path_file))
                        {
                            string jstonToWrite = (filePath as SSpaceFile).Deserialized?.Root?.ToJson();
                            if (!string.IsNullOrEmpty(jstonToWrite))
                                File.WriteAllText(path_file, jstonToWrite);
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
                        CompletedUnits++;
                    }

                }
            });
        }
    }
}
