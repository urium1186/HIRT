using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.ViewModels.Online;
using HaloInfiniteResearchTools.Views;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace HaloInfiniteResearchTools.ViewModels
{
    public enum MainSubView
    {
        Saludos,
        Online,
        Riping
    }
    public class MainViewModel : ViewModel
    {
        private FileContextModel _fileContext;
        #region Data Members

        private readonly ITabService _tabService;
        public ICommand OpenFileTabCommand { get; }
        public ICommand FileTreeExportJsonCommand { get; }
        public ICommand FileTreeExportRecursiveJsonCommand { get; }

        public ICommand OpenFileCommand { get; }
        public ICommand OpenFromRuntimeCommand { get; }
        public ICommand OpenDirectoryCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ResetSessionCommand { get; }

        public ICommand EditPreferencesCommand { get; }
        public ICommand TagStructsDumperCommand { get; }
        public ICommand TagStructsLoadAllCommand { get; }
        public ICommand BinaryExplorerCommand { get; }
        public ICommand ToolsCommand { get; }

        public ICommand BulkExportModelsCommand { get; }
        public ICommand BulkExportTexturesCommand { get; }
        public ICommand OpenViewSaludosCommand { get; }
        public ICommand OpenViewOnlineCommand { get; }
        public ICommand OpenViewRippingCommand { get; }

        public bool ShowSaludos { get { return currentVIew == MainSubView.Saludos; } }
        public bool ShowOnline { get { return currentVIew == MainSubView.Online; } }
        public bool ShowRiping { get { return currentVIew == MainSubView.Riping; } }

        MainSubView currentVIew = MainSubView.Saludos;

        public MainViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _fileContext = new FileContextModel();
            currentVIew = MainSubView.Saludos;
            _tabService = serviceProvider.GetService<ITabService>();
            TabContext = _tabService?.TabContext;
            SaludosViewModelP = new SaludosViewModel(serviceProvider);
            SaludosViewModelP.onChangeView += SaludosViewModelP_onChangeView;

            CustomOnlineViewModel = new CustomOnlineViewModel(serviceProvider);

            OpenFileTabCommand = new AsyncCommand<(IHIRTFile, bool)>(OpenFileTab);

            OpenFileCommand = new AsyncCommand(OpenFile);
            FileTreeExportJsonCommand = new AsyncCommand<List<IHIRTFile>>(FileTreeExportJson);
            FileTreeExportRecursiveJsonCommand = new AsyncCommand<IHIRTFile>(FileTreeExportRecursiveJson);
            OpenFromRuntimeCommand = new AsyncCommand(OpenFromRuntime);
            OpenDirectoryCommand = new AsyncCommand(OpenDirectory);
            EditPreferencesCommand = new AsyncCommand(EditPreferences);

            TagStructsDumperCommand = new AsyncCommand(TagStructsDumper);
            TagStructsLoadAllCommand = new AsyncCommand(TagStructsLoadAll);
            BinaryExplorerCommand = new AsyncCommand(BinaryExplorer);
            ToolsCommand = new AsyncCommand(Tools);

            ResetSessionCommand = new Command(ResetSession);

            BulkExportModelsCommand = new AsyncCommand(BulkExportModels);
            BulkExportTexturesCommand = new AsyncCommand(BulkExportTextures);

            OpenViewSaludosCommand = new Command(OpenViewSaludos);
            OpenViewRippingCommand = new Command(OpenViewRipping);
            OpenViewOnlineCommand = new Command(OpenViewOnline);

            App.Current.DispatcherUnhandledException += OnUnhandledExceptionRaised;
        }

        private void SaludosViewModelP_onChangeView(object? sender, MainSubView e)
        {
            changeCurrentView(e);
        }

        private void OpenViewOnline()
        {
            changeCurrentView(MainSubView.Online);
        }

        private void OpenViewRipping()
        {
            changeCurrentView(MainSubView.Riping);
        }

        private void OpenViewSaludos()
        {
            changeCurrentView(MainSubView.Saludos);
        }

        private void ResetSession()
        {
            _tabService.CloseAllTab();

            if (FileContext != null)
            {
                _fileContext.reset();
            }
            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private async Task FileTreeExportJson(List<IHIRTFile> files)
        {
            if (files is null) { return; }
            //dirModel.GetAllFiles(files);
            var prefs = GetPreferences();
            var process = new ExportFilesToJsonProcess(files, prefs.DefaultExportPath);
            await RunProcess(process);
        }

        private async Task FileTreeExportRecursiveJson(IHIRTFile file)
        {
            if (file == null) { return; }


            var prefs = GetPreferences();
            var process = new ExportFilesRecursiveToJsonProcess(file, prefs.DefaultExportPath);
            await RunProcess(process);
        }


        #endregion

        public FileContextModel FileContext { get => _fileContext; }

        public TabContextModel TabContext { get; }
        public SaludosViewModel SaludosViewModelP { get; }
        public CustomOnlineViewModel CustomOnlineViewModel { get; }

        #region Overrides

        protected override async Task OnInitializing()
        {
            var prefs = GetPreferences();
            UpdateHIRTContexConfigsFromPreference();
            _tabService.createHomeTab();
            if (prefs.LoadHIDirectoryOnStartup)
            {
                if (Directory.Exists(prefs.HIDirectoryPath))
                {
                    currentVIew = MainSubView.Riping;
                    var process = new OpenFilesProcess(null, prefs.HIDirectoryPath);
                    process.Completed += OpenFilesProcess_Completed;
                    changeCurrentView(MainSubView.Riping);
                    await RunProcess(process);
                }
            }
        }

        private void OpenFilesProcess_Completed(object? sender, EventArgs e)
        {
            //FileContext.SearchTermChangedCommand.Execute("ocgd");
            /*var see = LibHIRT.Utils.UIDebug.debugValues;
            var intese =LibHIRT.Utils.UIDebug.debugValues["unk0x08"]["3"].Intersect(LibHIRT.Utils.UIDebug.debugValues["unk0x08"]["2"]);*/

            //FileContext.SearchTermChangedCommand.Execute("C9CD0000_52681");
            //FileContext.SearchTermChangedCommand.Execute("");
        }

        private void changeCurrentView(MainSubView subView)
        {
            this.currentVIew = subView;
            OnPropertyChanged("ShowSaludos");
            OnPropertyChanged("ShowOnline");
            OnPropertyChanged("ShowRiping");
        }

        #endregion

        #region Private Methods

        private async Task OpenFile()
        {
            var filePaths = await ShowOpenFileDialog(
              title: "Open File",
              initialDirectory: GetPreferences().HIDirectoryPath); // TODO: Add filter

            if (filePaths == null)
                return;

            var process = new OpenFilesProcess(null, filePaths);
            changeCurrentView(MainSubView.Riping);
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
              defaultPath: GetPreferences().HIDirectoryPath);

            if (directoryPath == null)
                return;

            var process = new OpenFilesProcess(null, directoryPath);
            changeCurrentView(MainSubView.Riping);
            process.Completed += OpenFilesProcess_Completed;
            await RunProcess(process);
        }


        private async Task OpenFileTab((IHIRTFile, bool) fileP)
        {
            if (fileP.Item1 != null)
            {
                var file = fileP.Item1;
                if (!_tabService.CreateTabForFile(file, out _, fileP.Item2))
                {
                    var fileExt = Path.GetExtension(file.Name);
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
            var result = await ShowViewModal<PreferencesView>();
            if (result != null && result is HaloInfiniteResearchTools.Models.PreferencesModel)
            {
                UpdateHIRTContexConfigsFromPreference();
                await SavePreferences();
            }

        }

        private void UpdateHIRTContexConfigsFromPreference()
        {
            try
            {
                foreach (var item in GetPreferences().TagReaderOptionsModel.Paths)
                {
                    if (item.Active)
                    {
                        HIFileContext.Instance.TagTemplatePath = item.Path;
                        break;
                    }
                }
                HIFileContext.Instance.ConfigHIRT.LoadSaveFilesPath = GetPreferences().LoadSavePathFilesFromDB;
            }
            catch (Exception exp)
            {


            }

        }

        private async Task BinaryExplorer()
        {
            var result = await ShowViewModal<ToolBinaryExplorerView>(vm =>
            {
                var viewModel = vm as ToolBinaryExplorerViewModel;
            });
        }

        private async Task Tools()
        {
            var result = await ShowViewModal<ToolsView>(vm =>
            {
                var viewModel = vm as ToolsViewModel;
            });
        }

        private async Task TagStructsLoadAll()
        {

            var process = new TagStructsLoadAllProcess();
            await RunProcess(process);

        }

        private async Task TagStructsDumper()
        {
            var result = await ShowViewModal<TagStructsDumperView>(vm =>
            {
                var viewModel = vm as TagStructsDumperViewModel;
            });
            if (!(result is TagStructsDumperOptionsModel options))
                return;
            GetPreferences().TagReaderOptionsModel.XmlOutputPath = options.OutputPath;
            UpdateHIRTContexConfigsFromPreference();


            await SavePreferences();
            var exportProcess = new TagStructsDumperProcess(options);
            await RunProcess(exportProcess);
            if (exportProcess.OptionsModel.LastStartAddress != GetPreferences().TagStructsDumperOptions.LastStartAddress)
            {
                GetPreferences().TagStructsDumperOptions.LastStartAddress = exportProcess.OptionsModel.LastStartAddress;
                UpdateHIRTContexConfigsFromPreference();
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
