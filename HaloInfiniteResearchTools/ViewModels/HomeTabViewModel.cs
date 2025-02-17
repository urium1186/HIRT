using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HaloInfiniteResearchTools.ViewModels
{
    public class HomeTabViewModel : ViewModel, IDisposeWithView
    {
        public ICommand OpenModulesDirectoryCommand { get; }
        public ICommand OpenModuleFileCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand OpenOcgdCommand { get; }
        public ICommand FilterBipdCommand { get; }
        public ICommand FilterCharCommand { get; }
        public ICommand FilterWeapCommand { get; }
        public ICommand FilterVehiCommand { get; }
        public ICommand FilterHlmtCommand { get; }
        public ICommand FilterModeCommand { get; }
        public ICommand FilterLevlCommand { get; }
        public ICommand FilterSbspCommand { get; }
        public HomeTabViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            OpenModuleFileCommand = new AsyncCommand(OpenFile);
            OpenModulesDirectoryCommand = new AsyncCommand(OpenDirectory);
            OpenOcgdCommand = new AsyncCommand(OpenOcgd);
            ClearFiltersCommand = new AsyncCommand(ClearFilters);
            FilterBipdCommand = new AsyncCommand(FilterBipd);
            FilterCharCommand = new AsyncCommand(FilterChar);
            FilterWeapCommand = new AsyncCommand(FilterWeap);
            FilterVehiCommand = new AsyncCommand(FilterVehi);
            FilterHlmtCommand = new AsyncCommand(FilterHlmt);
            FilterModeCommand = new AsyncCommand(FilterMode);
            FilterLevlCommand = new AsyncCommand(FilterLevl);
            FilterSbspCommand = new AsyncCommand(FilterSbsp);
        }

        private async Task ClearFilters()
        {
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel?.FileContext.SearchTermChangedCommand.Execute("");
        }

        private async Task OpenOcgd()
        {
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel?.FileContext.SearchTermChangedCommand.Execute("ocgd");
        }
        private async Task FilterBipd()
        {
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel?.FileContext.SearchTermChangedCommand.Execute("bipd");
        }
        private async Task FilterChar()
        {
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel?.FileContext.SearchTermChangedCommand.Execute("char");
        }
        private async Task FilterWeap()
        {
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel?.FileContext.SearchTermChangedCommand.Execute("weap");
        }
        private async Task FilterVehi()
        {
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel?.FileContext.SearchTermChangedCommand.Execute("vehi");
        }
        private async Task FilterHlmt()
        {
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel?.FileContext.SearchTermChangedCommand.Execute("hlmt");
        }
        private async Task FilterMode()
        {
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel?.FileContext.SearchTermChangedCommand.Execute("mode");
        }
        private async Task FilterLevl()
        {
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel?.FileContext.SearchTermChangedCommand.Execute("levl");
        }
        private async Task FilterSbsp()
        {
            var mainViewModel = ServiceProvider.GetRequiredService<MainViewModel>();
            mainViewModel?.FileContext.SearchTermChangedCommand.Execute("sbsp");
        }

        private async Task OpenFile()
        {
            try
            {
                var filePaths = await ShowOpenFileDialog(
              title: "Open File",
              initialDirectory: GetPreferences().HIDirectoryPath,
              filter: "Modules files (*.module)|*.module"
              ); // TODO: Add filter

                if (filePaths == null)
                    return;

                var process = new OpenFilesProcess(null, filePaths);
                process.Completed += OpenFilesProcess_Completed;
                await RunProcess(process);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private void OpenFilesProcess_Completed(object? sender, EventArgs e)
        {

        }

        private async Task OpenDirectory()
        {
            var directoryPath = await ShowFolderBrowserDialog(
              title: "Open Directory",
              defaultPath: GetPreferences().HIDirectoryPath);

            if (directoryPath == null)
                return;

            var process = new OpenFilesProcess(null, directoryPath);
            process.Completed += OpenFilesProcess_Completed;
            await RunProcess(process);
        }


    }
}
