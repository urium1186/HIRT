using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Services.Abstract;
using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.UI.Modals;
using HaloInfiniteResearchTools.ViewModels;
using HaloInfiniteResearchTools.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LibHIRT.Files;

namespace HaloInfiniteResearchTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Data Members

        private IServiceProvider _serviceProvider;

        #endregion

        #region Properties

        public IServiceProvider ServiceProvider
        {
            get => _serviceProvider;
        }

        public MainViewModel MainViewModel { get; private set; }

        #endregion

        #region Constructor

        public App()
        {
        }

        #endregion

        #region Private Methods

        private void ConfigureDependencies(IServiceCollection services)
        {
            ConfigureModals(services);
            ConfigureViews(services);
            ConfigureViewModels(services);
            ConfigureWindows(services);

            ConfigureServices(services);
        }

        private void ConfigureModals(IServiceCollection services)
        {
            services.AddTransient<MessageModal>();
            services.AddTransient<ProgressModal>();
        }

        private void ConfigureViews(IServiceCollection services)
        {
            services.AddTransient<MainView>();
            

            services.AddTransient<AboutView>();
            services.AddTransient<TagStructsDumperView>();
            services.AddTransient<ModelExportOptionsView>();
            services.AddTransient<RenderModelView>();
            services.AddTransient<ModelView>();
            services.AddTransient<ShaderBytecodeView>();
            services.AddTransient<CustomizationGlobalsDefinitionView>();
            services.AddTransient<GenericView>();
            services.AddTransient<LevelView>();
            services.AddTransient<PreferencesView>();
            services.AddTransient<StatusListView>();
            services.AddTransient<TextEditorView>();
            services.AddTransient<TextureView>();
            services.AddTransient<TextureExportOptionsView>();
            services.AddTransient<ToolBinaryExplorerView>();
            services.AddTransient<ToolsView>();
            services.AddTransient<ScenarioStructureBspView>();
        }

        private void ConfigureViewModels(IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();

            services.AddTransient<DefaultViewModel>();
            services.AddTransient<AboutViewModel>();
            services.AddTransient<TagStructsDumperViewModel>();
            services.AddTransient<ModelExportOptionsViewModel>();
            services.AddTransient<GenericViewModel>();
            services.AddTransient<LevelViewModel>();
            services.AddTransient<RenderModelViewModel>();
            services.AddTransient<ModelViewModel>();
            services.AddTransient<ShaderBytecodeViewModel>();
            services.AddTransient<CustomizationGlobalsDefinitionViewModel>();
            services.AddTransient<PreferencesViewModel>();
            services.AddTransient<ProgressViewModel>();
            services.AddTransient<StatusListViewModel>();
            services.AddTransient<TextEditorViewModel>();
            services.AddTransient<TextureViewModel>();
            services.AddTransient<TextureExportOptionsViewModel>();
            services.AddTransient<ToolBinaryExplorerViewModel>();
            services.AddTransient<ToolsViewModel>();
            services.AddTransient<ScenarioStructureBspViewModel>();
        }

        private void ConfigureWindows(IServiceCollection services)
        {
            services.AddTransient<MainWindow>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHIFileContext, HIFileContext>();

            services.AddSingleton<IFileTypeService, FileTypeService>();
            services.AddSingleton<IMeshIdentifierService, MeshIdentifierService>();
            services.AddSingleton<IPreferencesService, PreferencesService>();
            services.AddSingleton<ITabService, TabService>();

            services.AddTransient<IFileDialogService, FileDialogService>();
            services.AddTransient<ITextureConversionService, TextureConversionService>();
            services.AddTransient<IViewService, ViewService>();
        }

        #endregion

        #region Event Handlers

        private async void OnAppStartup(object sender, StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureDependencies(services);
            _serviceProvider = services.BuildServiceProvider();

            await _serviceProvider.GetRequiredService<IPreferencesService>().Initialize();

            var window = _serviceProvider.GetService<MainWindow>();
            MainViewModel = (MainViewModel)window.DataContext;

            window.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = false;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(string.Format("Is Terminating: {0}\r\n{1}", e.IsTerminating, e.ExceptionObject.ToString()));
        }

        #endregion

    }
}
