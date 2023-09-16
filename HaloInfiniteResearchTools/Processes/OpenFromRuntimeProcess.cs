using LibHIRT.Files;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    internal class OpenFromRuntimeProcess : ProcessBase<IEnumerable<string>>
    {
        private readonly HIFileContext _fileContext;
        private readonly string[] _inputPaths;
        private readonly List<string> _filesLoaded;

        public override IEnumerable<string> Result => _filesLoaded;

        public OpenFromRuntimeProcess(params string[] paths)
        {
            _fileContext = (HIFileContext?)ServiceProvider.GetRequiredService<IHIFileContext>();
            _inputPaths = paths;

            _filesLoaded = new List<string>();
        }

        protected override async Task OnInitializing()
        {

        }
        protected override async Task OnExecuting()
        {
            Status = "Loading File";
            UnitName = "file loaded";
            TotalUnits = 1;
            IsIndeterminate = true;

            var objLock = new object();
            var _files = new List<string>();
            _files.Add("");
            Parallel.ForEach(_files, file =>
            {
                try
                {
                    if (!_fileContext.OpenFromRuntime(""))
                        StatusList.AddWarning("memory", "Failed to open file.");
                    else
                    {
                        while (!_fileContext.RuntimeLoadCompleted)
                        {

                        }


                        _filesLoaded.Add("memory");
                        StatusList.AddMessage("memory", "Open file.");
                    }

                }
                catch (Exception ex)
                {
                    StatusList.AddError("memory", ex);
                }
                finally
                {
                    lock (objLock)
                        CompletedUnits++;
                }
            });
        }
    }
}
