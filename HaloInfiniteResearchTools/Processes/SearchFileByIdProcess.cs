using LibHIRT.Files;
using LibHIRT.Files.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    public class SearchFileByIdProcess : ProcessBase<IEnumerable<IHIRTFile>>
    {

        #region Data Members

        private readonly IHIFileContext _fileContext;

        private IEnumerable<string> _inputPaths;
        private bool _load_resource;
        private int _id;
        private string[] _filePaths;

        private List<IHIRTFile> _filesLoaded;

        #endregion

        #region Properties

        public override IEnumerable<IHIRTFile> Result
        {
            get => _filesLoaded;
        }

        #endregion

        #region Constructor

        public SearchFileByIdProcess(IServiceProvider? serviceProvider, int id, bool load_resource, params string[] paths) : base(serviceProvider)
        {
            _fileContext = HIFileContext.Instance;
            _inputPaths = paths;
            _load_resource = load_resource;
            _id = id;

            _filesLoaded = new List<IHIRTFile>();
        }

        #endregion

        #region Overrides

        protected override async Task OnInitializing()
        {
            _filePaths = GetFilePaths().ToArray();
        }

        protected override async Task OnExecuting()
        {
            Status = _filePaths.Length > 1 ? "Opening Files" : "Opening File";
            UnitName = _filePaths.Length > 1 ? "files opened" : "file opened";
            TotalUnits = _filePaths.Length;
            IsIndeterminate = _filePaths.Length == 1;

            var objLock = new object();
            Parallel.ForEach(_filePaths, (filePath, state) =>
            {
                var fileName = Path.GetFileName(filePath);
                var fi = new FileInfo(filePath);

                string temp = filePath.Replace(@"C:\Program Files (x86)\Steam\steamapps\common\Halo Infinite\deploy\", "") + " " + fi.Length.ToString();
                Status = Status + "\n" + temp;
                try
                {

                    var fileR = _fileContext.OpenFileWithIdInModule(filePath, _id, _load_resource);
                    if (fileR == null)
                    {

                        StatusList.AddWarning(fileName, "Failed to open file.");
                    }
                    else
                    {
                        Status = Status.Replace("\n" + temp, "");
                        _filesLoaded.Add(fileR);
                        StatusList.AddMessage(fileName, "Open file.");
                        state.Stop();
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
                        Status = Status.Replace("\n" + temp, "");
                        CompletedUnits++;
                    }

                }
            });
        }

        #endregion

        #region Private Methods

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
                    var directoryFiles = Directory.GetFiles(currentPath, "*.*", SearchOption.AllDirectories)
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
            return !HIFileContext.NoSupportedFileExtensions.Contains(ext) && (HIFileContext.SupportedFileExtensions.Contains(ext) || HIFileContext.SupportedFileExtensions.Contains(".*"));
        }

        #endregion

    }
}
