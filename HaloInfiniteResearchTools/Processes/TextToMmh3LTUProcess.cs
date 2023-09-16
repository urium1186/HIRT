using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    public class TextToMmh3LTUProcess : ProcessBase<IEnumerable<string>>
    {
        private IEnumerable<string> _inputPaths;
        private string[] _filePaths;
        private List<string> _filesLoaded;
        private string _spliters;
        private bool _forceLowerCase;
        private bool _prevForceLowerCase;
        private bool _prevCalueUOIU;
        private bool _dbModify;

        public TextToMmh3LTUProcess(IEnumerable<string>? paths, string spliters, bool forceLowerCase)
        {

            _inputPaths = paths;

            _filesLoaded = new List<string>();

            _spliters = spliters;
            _forceLowerCase = forceLowerCase;
        }

        public override IEnumerable<string> Result
        {
            get => _filesLoaded;
        }
        public bool DbModify { get => _dbModify; set => _dbModify = value; }

        protected override async Task OnExecuting()
        {
            Status = _filePaths.Length > 1 ? "Opening Files" : "Opening File";
            UnitName = _filePaths.Length > 1 ? "files opened" : "file opened";
            TotalUnits = _filePaths.Length;
            IsIndeterminate = _filePaths.Length == 1;
            _prevCalueUOIU = Mmr3HashLTU.UpdateOnlyInUse;
            Mmr3HashLTU.UpdateOnlyInUse = true;

            _prevForceLowerCase = Mmr3HashLTU.ForceLower;
            Mmr3HashLTU.ForceLower = _forceLowerCase;

            _dbModify = false;
            var objLock = new object();
            Parallel.ForEach(_filePaths, filePath =>
            {
                var fileName = Path.GetFileName(filePath);
                var fi = new FileInfo(filePath);

                string temp = filePath.Replace(@"C:\Program Files (x86)\Steam\steamapps\common\Halo Infinite\deploy\", "") + " " + fi.Length.ToString();
                Status = Status + "\n" + temp;

                try
                {

                    StreamReader sr = new StreamReader(filePath);
                    while (!sr.EndOfStream)
                    {
                        string tempLine = sr.ReadLine().Replace("\0", "");
                        if (string.IsNullOrEmpty(tempLine))
                            continue;
                        //char[] c_array = new char[] { '\r', '\n', ' ', '\t' };
                        //tempLine = tempLine.Replace(" ", "_");
                        var splits = tempLine.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        //var splits = tempLine.Split(c_array,StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        foreach (string line in splits)
                        {
                            try
                            {
                                SpecialOperations(line);
                                if (Mmr3HashLTU.AddUniqueStrValue(line))
                                {
                                    if (!_dbModify)
                                        _dbModify = true;
                                }
                                if (line.Contains("_textureIndex"))
                                {
                                    if (Mmr3HashLTU.AddUniqueStrValue(line.Replace("_textureIndex", "")))
                                    {
                                        if (!_dbModify)
                                            _dbModify = true;
                                    }
                                }
                                if (line.Contains("_texture_textureIndex"))
                                {
                                    if (Mmr3HashLTU.AddUniqueStrValue(line.Replace("_texture_textureIndex", "")))
                                    {
                                        if (!_dbModify)
                                            _dbModify = true;
                                    }
                                }
                                char[] c_array2 = _spliters.ToCharArray();
                                foreach (char c in c_array2)
                                {
                                    SplitEntryBy(line, c);
                                }
                                //SplitEntryBy(line, '_');
                                //SplitEntryBy(line, '/');
                                //SplitEntryBy(line,'\\');
                                //SplitEntryBy(line,'.');
                                //SplitEntryBy(line, ':');

                                string fileName_temp = Path.GetFileNameWithoutExtension(line);
                                if (Mmr3HashLTU.AddUniqueStrValue(fileName_temp))
                                {
                                    if (!_dbModify)
                                        _dbModify = true;
                                }
                            }
                            catch (Exception exp)
                            {

                                continue;
                            }

                        }
                    }
                    sr.Close();

                }
                catch (Exception ex)
                {
                    StatusList.AddError(fileName, ex);
                }
                finally
                {
                    lock (objLock)
                    {
                        Status = Status.Replace("\n" + temp, "");
                        CompletedUnits++;
                    }

                }
            });
        }

        private void SpecialOperations(string line)
        {
            if (!line.Contains("_default_ps"))
                return;
            string val_1 = line.Split("_default_ps@@")[0];
            var array_S = val_1.Split("?");
            if (array_S.Length > 1)
            {
                string val_2 = array_S[array_S.Length - 1];
                if (Mmr3HashLTU.AddUniqueStrValue(val_2))
                {
                    if (!_dbModify)
                        _dbModify = true;
                }
            }

        }

        private void SplitEntryBy(string line, char splitter)
        {
            var splet = line.Split(splitter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (splet.Length <= 1)
                return;
            foreach (string word in splet)
            {
                if (Mmr3HashLTU.AddUniqueStrValue(word))
                {
                    if (!_dbModify)
                        _dbModify = true;
                }
            }
        }

        protected override async Task OnInitializing()
        {
            _filePaths = GetFilePaths().ToArray();
        }

        private IEnumerable<string> GetFilePaths()
        {
            var visitedSet = new HashSet<string>();
            var queue = new Queue<string>(_inputPaths);
            while (queue.TryDequeue(out var currentPath))
            {
                if (!visitedSet.Add(currentPath))
                    continue;

                if (!File.Exists(currentPath) && !Directory.Exists(currentPath))
                {
                    StatusList.AddWarning(currentPath, "Path does not exist. Skipping.");
                    continue;
                }

                var attributes = File.GetAttributes(currentPath);
                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    var directoryFiles = Directory.GetFiles(currentPath, "*.txt", SearchOption.AllDirectories)
                      .Where(IsFileExtensionRecognized);

                    foreach (var file in directoryFiles)
                        queue.Enqueue(file);

                    continue;
                }
                else
                {
                    if (!IsFileExtensionRecognized(currentPath))
                    {
                        StatusList.AddWarning(currentPath, "File extension is not recognized. Skipping.");
                        continue;
                    }

                    yield return currentPath;
                }
            }
        }

        private static bool IsFileExtensionRecognized(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            return ext == ".txt";
        }

        protected override Task OnComplete()
        {
            Mmr3HashLTU.UpdateOnlyInUse = _prevCalueUOIU;
            Mmr3HashLTU.ForceLower = _prevForceLowerCase;
            return base.OnComplete();
        }
    }
}
