using LibHIRT.Grunt.Converters;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    public class ItemJsonToMmh3LTUProcess : ProcessBase<IEnumerable<string>>
    {
        private IEnumerable<string> _inputPaths;
        private string[] _filePaths;
        private List<string> _filesLoaded;
        private string _spliters;
        private bool _forceLowerCase;
        private bool _prevForceLowerCase;
        private bool _prevCalueUOIU;
        private bool _dbModify;

        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                (JsonConverter)new EmptyDateStringToNullJsonConverter(),
                (JsonConverter)new OnlineUriReferenceConverter(),
                (JsonConverter)new AcknowledgementTypeConverter(),
                (JsonConverter)new XmlDurationToTimeSpanJsonConverter()
            }
        };

        public ItemJsonToMmh3LTUProcess(IEnumerable<string>? paths, string spliters, bool forceLowerCase)
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
            Mmr3HashLTU.UpdateOnlyInUse = false;

            _prevForceLowerCase = Mmr3HashLTU.ForceLower;
            Mmr3HashLTU.ForceLower = _forceLowerCase;

            _dbModify = false;
            var objLock = new object();
            Parallel.ForEach(_filePaths, filePath =>
            {
                var fileName = Path.GetFileName(filePath);
                var fi = new FileInfo(filePath);

                
                Status = "Procesando ...";

                try
                {
                    string jsonString_temp = System.IO.File.ReadAllText(filePath);
                    var documento = JsonDocument.Parse(jsonString_temp);
                    var valor = documento.RootElement.GetProperty("CommonData").GetProperty("AltName").GetProperty("value").GetString(); //JsonSerializer.Deserialize(jsonString_temp, ret_type, serializerOptions);

                    
                    if (Mmr3HashLTU.AddUniqueStrValue(valor))
                    {
                        if (!_dbModify)
                            _dbModify = true;
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
                        //Status = Status.Replace("\n" + temp, "");
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
                    var directoryFiles = Directory.GetFiles(currentPath, "*.json", SearchOption.AllDirectories)
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
            return ext == ".json";
        }

        protected override Task OnComplete()
        {
            Mmr3HashLTU.UpdateOnlyInUse = _prevCalueUOIU;
            Mmr3HashLTU.ForceLower = _prevForceLowerCase;
            return base.OnComplete();
        }
    }
}
