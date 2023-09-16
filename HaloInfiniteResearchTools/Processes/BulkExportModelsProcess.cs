using HaloInfiniteResearchTools.Models;
using LibHIRT.Files;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace HaloInfiniteResearchTools.Processes
{

    public class BulkExportModelsProcess : ProcessBase
    {

        #region Data Members

        private IHIFileContext _fileContext;

        private IEnumerable<SSpaceFile> _targetFiles; //TplFile
        private IList<ExportModelProcess> _processes;

        private ModelExportOptionsModel _modelOptions;
        private TextureExportOptionsModel _textureOptions;

        #endregion

        #region Properties

        public override bool CanCancel => true;

        #endregion

        #region Constructor

        public BulkExportModelsProcess(ModelExportOptionsModel modelOptions, TextureExportOptionsModel textureOptions)
        {
            _modelOptions = modelOptions;
            _textureOptions = textureOptions;

            _fileContext = ServiceProvider.GetRequiredService<IHIFileContext>();
        }

        #endregion

        #region Overrides

        protected override async Task OnInitializing()
        {
            IsIndeterminate = true;
            Status = "Preparing Bulk Export";

            if (_targetFiles is null)
                _targetFiles = GatherFiles();

            _processes = CreateProcesses(_targetFiles);
        }

        protected override async Task OnExecuting()
        {
            Status = "Exporting Models";
            CompletedUnits = 0;
            TotalUnits = _processes.Count;
            UnitName = "Models Exported";
            IsIndeterminate = false;

            if (IsCancellationRequested)
                return;

            var processLock = new object();
            var executionBlock = new ActionBlock<ExportModelProcess>(async process =>
            {
                if (IsCancellationRequested)
                    return;

                await process.Execute();

                lock (processLock)
                    CompletedUnits++;
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Math.Min(1, Environment.ProcessorCount),
                EnsureOrdered = false
            });

            foreach (var process in _processes)
                executionBlock.Post(process);

            executionBlock.Complete();
            await executionBlock.Completion;

            foreach (var process in _processes)
                StatusList.Merge(process.StatusList);
        }

        protected override async Task OnComplete()
        {
            _targetFiles = null;
            _processes = null;
        }

        #endregion

        #region Private Methods

        private IEnumerable<SSpaceFile> GatherFiles()
        {
            string[] filters = null;
            if (!string.IsNullOrWhiteSpace(_modelOptions.Filters))
                filters = _modelOptions.Filters.Split(';', System.StringSplitOptions.RemoveEmptyEntries);

            return _fileContext.GetFiles<SSpaceFile>().Where(file =>
            {
                if (filters is null)
                    return true;

                foreach (var filter in filters)
                    if (file.Name.ToLower().Contains(filter.ToLower()))
                        return true;

                return false;
            });
        }

        private IList<ExportModelProcess> CreateProcesses(IEnumerable<SSpaceFile> files)
          => files.Select(file => new ExportModelProcess(file, _modelOptions, _textureOptions)).ToList();

        #endregion

    }

}
