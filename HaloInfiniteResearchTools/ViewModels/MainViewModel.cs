using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.Views;
using LibHIRT.Files;
using LibHIRT.TagReader.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace HaloInfiniteResearchTools.ViewModels
{
    public class MainViewModel : ViewModel
    {
        #region Data Members

        private readonly ITabService _tabService;
        public ICommand OpenFileTabCommand { get; }

        public ICommand OpenFileCommand { get; }
        public ICommand OpenFromRuntimeCommand { get; }
        public ICommand OpenDirectoryCommand { get; }
        public ICommand ExitCommand { get; }

        public ICommand EditPreferencesCommand { get; }
        public ICommand TagStructsDumperCommand { get; }
        public ICommand BinaryExplorerCommand { get; }

        public ICommand BulkExportModelsCommand { get; }
        public ICommand BulkExportTexturesCommand { get; }


        public MainViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            FileContext = new FileContextModel();

            _tabService = serviceProvider.GetService<ITabService>();
            TabContext = _tabService.TabContext;
            OpenFileTabCommand = new AsyncCommand<IFileModel>(OpenFileTab);

            OpenFileCommand = new AsyncCommand(OpenFile);
            OpenFromRuntimeCommand = new AsyncCommand(OpenFromRuntime);
            OpenDirectoryCommand = new AsyncCommand(OpenDirectory);
            EditPreferencesCommand = new AsyncCommand(EditPreferences);

            TagStructsDumperCommand = new AsyncCommand(TagStructsDumper);
            BinaryExplorerCommand = new AsyncCommand(BinaryExplorer);

            BulkExportModelsCommand = new AsyncCommand(BulkExportModels);
            BulkExportTexturesCommand = new AsyncCommand(BulkExportTextures);

            App.Current.DispatcherUnhandledException += OnUnhandledExceptionRaised;
        }

       
        #endregion

        public FileContextModel FileContext { get; }

        public TabContextModel TabContext { get; }

        #region Overrides

        protected override async Task OnInitializing()
        {
            var prefs = GetPreferences();
            //FileContext.HiContext.TagTemplatePath = prefs.TagStructsDumperOptions.OutputPath;
            if (prefs.LoadH2ADirectoryOnStartup)
            {
                if (Directory.Exists(prefs.H2ADirectoryPath))
                {
                    var process = new OpenFilesProcess(prefs.H2ADirectoryPath);
                    process.Completed += OpenFilesProcess_Completed;
                    await RunProcess(process);
                }
            }
        }

        private void OpenFilesProcess_Completed(object? sender, EventArgs e)
        {
            FileContext.SearchTermChangedCommand.Execute("");
        }

        #endregion

        #region Private Methods

        private async Task OpenFile()
        {
            var filePaths = await ShowOpenFileDialog(
              title: "Open File",
              initialDirectory: GetPreferences().H2ADirectoryPath); // TODO: Add filter

            if (filePaths == null)
                return;

            var process = new OpenFilesProcess(filePaths);

            process.Completed += OpenFilesProcess_Completed;
            await RunProcess(process);
        }
        private async Task OpenFromRuntime()
        {
            var process = new OpenFromRuntimeProcess(null);
            process.Completed += OpenFromRuntimeProcess_Completed;
            await RunProcess(process);
        }

        private void OpenFromRuntimeProcess_Completed(object? sender, EventArgs e)
        {
            FileContext.fillMemFilesbyGroup();

        }

        private async Task OpenDirectory()
        {
            var directoryPath = await ShowFolderBrowserDialog(
              title: "Open Directory",
              defaultPath: GetPreferences().H2ADirectoryPath);

            if (directoryPath == null)
                return;

            var process = new OpenFilesProcess(directoryPath);

            process.Completed += OpenFilesProcess_Completed;
            await RunProcess(process);
        }


        private async Task OpenFileTab(IFileModel fileModel)
        {
            var f_m = fileModel as FileModel;
            if (f_m != null) {
                var file = f_m.File;
                if (!_tabService.CreateTabForFile(file, out _, f_m.GenericView))
                {
                    var fileExt = Path.GetExtension(file.Name);
                    await ShowMessageModal(
                      title: "Unsupported File Type",
                      message: $"We can't open {fileExt} files yet.");

                    return;
                }
            }
            var h_m = fileModel as TreeHierarchicalModel;
            if (h_m != null)
            {
                if (!_tabService.CreateTabForFile((TagStructMem)h_m.Value, out _,true))
                {
                    var fileExt = Path.GetExtension(((TagStructMem)h_m.Value).TagGroup);
                    await ShowMessageModal(
                      title: "Unsupported File Type",
                      message: $"We can't open {fileExt} files yet.");

                    return;
                }
            }

                /*var entry = new List<FileModel>();
                entry.Add(fileModel);
                var process = new OpenModuleEntryFileProcess(entry);
                await RunProcess(process);
                MainWindow mv = (MainWindow)ServiceProvider.GetService(typeof(MainWindow));
                mv.UpdateJsonInTree();
                */
            }

        private async Task EditPreferences()
        {
            await ShowViewModal<PreferencesView>();
            await SavePreferences();
        }

        private async Task BinaryExplorer()
        {
            var result = await ShowViewModal<ToolBinaryExplorerView>(vm =>
            {
                var viewModel = vm as ToolBinaryExplorerViewModel;
            });
        }
         private async Task TagStructsDumper()
        {
            var result = await ShowViewModal<TagStructsDumperView>(vm =>
            {
                var viewModel = vm as TagStructsDumperViewModel;
            });
            if (!(result is TagStructsDumperOptionsModel options))
                return;
            this.FileContext.HiContext.TagTemplatePath = options.OutputPath;
            GetPreferences().TagReaderOptionsModel.OutputPath = options.OutputPath;

            await SavePreferences();
            var exportProcess = new TagStructsDumperProcess(options);
            await RunProcess(exportProcess);
            if (exportProcess.OptionsModel.LastStartAddress != GetPreferences().TagStructsDumperOptions.LastStartAddress)
            {
                GetPreferences().TagStructsDumperOptions.LastStartAddress = exportProcess.OptionsModel.LastStartAddress;
                await SavePreferences();
            }
        }



        private async Task BulkExportModels()
        {
            var result = await ShowViewModal<ModelExportOptionsView>(vm =>
            {
                var viewModel = vm as ModelExportOptionsViewModel;
                viewModel.IsForBatch = true;
            });

            if (!(result is Tuple<ModelExportOptionsModel, TextureExportOptionsModel> options))
                return;

            var modelOptions = options.Item1;
            var textureOptions = options.Item2;

            var exportProcess = new BulkExportModelsProcess(modelOptions, textureOptions);
            await RunProcess(exportProcess);
        }

        private async Task BulkExportTextures()
        {
            var result = await ShowViewModal<TextureExportOptionsView>(vm =>
            {
                var viewModel = vm as TextureExportOptionsViewModel;
                viewModel.IsForBatch = true;
            });

            if (!(result is TextureExportOptionsModel options))
                return;

            var exportProcess = new BulkExportTexturesProcess(options);
            await RunProcess(exportProcess);
        }

        #endregion

        #region Event Handlers

        private async void OnUnhandledExceptionRaised(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            await ShowExceptionModal(e.Exception);
        }

        #endregion
    }
}
