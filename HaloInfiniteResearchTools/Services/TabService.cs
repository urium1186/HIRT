using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.ViewModels;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader.RuntimeViewer;
using System;
using System.IO;
using System.Linq;

namespace HaloInfiniteResearchTools.Services
{

    public class TabService : ITabService
    {

        #region Data Members

        private readonly IServiceProvider _serviceProvider;
        private readonly IFileTypeService _fileTypeService;
        private readonly IViewService _viewService;

        private readonly TabContextModel _tabContext;

        #endregion

        #region Properties

        public TabContextModel TabContext
        {
            get => _tabContext;
        }

        #endregion

        #region Constructor

        public TabService(IServiceProvider serviceProvider,
          IFileTypeService fileTypeService,
          IViewService viewService)
        {
            _serviceProvider = serviceProvider;
            _fileTypeService = fileTypeService;
            _viewService = viewService;
            _tabContext = new TabContextModel();
        }

        #endregion

        #region Public Methods

        public bool CreateTabForFile(IHIRTFile file, out ITab tab, bool forceGeneric = false)
        {
            if (file is ISSpaceFile)
                return createTabSSpaceFile((ISSpaceFile)file, out tab, forceGeneric);
            else
                return createTabFileMem((TagStructMemFile)file, out tab);

        }

        private bool createTabSSpaceFile(ISSpaceFile file, out ITab tab, bool forceGeneric)
        {
            tab = default;

            if ((!forceGeneric && NavigateToTab(file.Name)))
            {
                tab = _tabContext.CurrentTab;
                return true;
            }
            else if (forceGeneric && NavigateToTab("GenericView_" + file.Name))
            {
                tab = _tabContext.CurrentTab;
                return true;
            }
            var _type = file.GetType();
            if (forceGeneric)
                _type = typeof(GenericFile);
            var viewModelType = _fileTypeService.GetViewModelType(_type);

            if (viewModelType is null)
            {
                _type = typeof(GenericFile);
                viewModelType = _fileTypeService.GetViewModelType(_type);
                if (viewModelType is null)
                    return false;
            }

            if (typeof(GenericFile) == viewModelType && NavigateToTab("GenericView_" + file.Name))
            {
                tab = _tabContext.CurrentTab;
                return true;
            }
            try
            {
                var viewModel = (IViewModel)Activator.CreateInstance(viewModelType, new object[] { _serviceProvider, file });
                viewModel.Initialize();

                var view = _viewService.GetView(viewModel);

                var fileName = Path.GetFileName(file.Name);
                if (viewModel is GenericViewModel)
                    fileName = "GenericView_" + fileName;
                tab = new Tab(fileName, view);

                _tabContext.AddTab(tab);

                return true;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public bool CreateTabForFile(TagStructMemFile file, out ITab tab, bool forceGeneric = false)
        {
            return createTabFileMem(file, out tab);
        }

        private bool createTabFileMem(TagStructMemFile file, out ITab tab)
        {
            tab = default;

            if (NavigateToTab(file.TagFullName))
            {
                tab = _tabContext.CurrentTab;
                return true;
            }
            try
            {
                var viewModel = (IViewModel)Activator.CreateInstance(typeof(GenericViewModel), new object[] { _serviceProvider, file });
                viewModel.Initialize();

                var view = _viewService.GetView(viewModel);

                var fileName = Path.GetFileName(file.TagFullName);
                tab = new Tab(fileName, view);

                _tabContext.AddTab(tab);

                return true;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public void CloseAllTab()
        {
            
            try
            {
                int count = _tabContext.Tabs.Count;
                for (int i = 0; i < count; i++)
                {
                    if (_tabContext.Tabs.Count>0)
                        _tabContext.Tabs.Last().Close();

                }
                
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public bool NavigateToTab(string tabName)
        {
            if (!TryFindTab(tabName, out var tab))
                return false;

            _tabContext.CurrentTab = tab;
            return true;
        }

        #endregion

        #region Private Methods

        private bool TryFindTab(string tabName, out ITab tab)
        {
            tab = _tabContext.Tabs.FirstOrDefault(x => x.Name == tabName);
            return tab != null;
        }


        #endregion

    }

}
