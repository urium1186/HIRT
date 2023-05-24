using HaloInfiniteResearchTools.Models;
using LibHIRT.Files;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    public class OpenModuleEntryFileProcess : ProcessBase<IEnumerable<string>>
    {
        private readonly IHIFileContext _fileContext;
        private List<string> _filesLoaded;
        private IEnumerable<FileModel> _files;

        public OpenModuleEntryFileProcess(IEnumerable<FileModel> files)
        {
            _files = files;
            _filesLoaded = new List<string>();
            _fileContext = ServiceProvider.GetRequiredService<IHIFileContext>();
        }

        public override IEnumerable<string> Result
        {
            get => _filesLoaded;
        }

        protected override async Task OnExecuting()
        {
            Status = "Loading File";
            UnitName = "file loaded";
            TotalUnits = 1;
            IsIndeterminate = true;

            var objLock = new object();
            Parallel.ForEach(_files, file =>
            {
                try
                {
                    if (!_fileContext.ReadTagOnFile(file.File))
                        StatusList.AddWarning(file.Name, "Failed to open file.");
                    else
                        _filesLoaded.Add(file.Name);
                }
                catch (Exception ex)
                {
                    StatusList.AddError(file.Name, ex);
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
