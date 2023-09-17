using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.ViewModels;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader.Common;
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

        public bool CreateTabForFile(ISSpaceFile file, out ITab tab, bool forceGeneric = false)
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

        public bool CreateTabForFile(TagStructMem file, out ITab tab, bool forceGeneric = false)
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
