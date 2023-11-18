using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.UI.Modals;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace HaloInfiniteResearchTools.ViewModels
{
    public class ModuleIndexFileFilter { 
        public string GlobalId { get; set;} 
        public string GlobalIdRefIn { get; set;} 
    }

    [AcceptsFileType(typeof(ModuleIndexFile))]
    public class ModuleIndexFileViewModel : SSpaceFileViewModel<ModuleIndexFile>, IDisposeWithView
    {
        private readonly ITabService _tabService;

        private readonly ICollectionView _collectionViewSource;

        public ICollectionView Ids
        {
            get => _collectionViewSource;
        }

        public ICommand OpenGenFileViewCommand { get; }
        public ICommand FilterViewCollectionCommand { get; }
        public ModuleIndexFileFilter Filters { get; set; }
        public ModuleIndexFileViewModel(IServiceProvider serviceProvider, ModuleIndexFile file) : base(serviceProvider, file)
        {
            OpenGenFileViewCommand = new AsyncCommand<int>(OpenGenFileTab);
            FilterViewCollectionCommand = new AsyncCommand(FilterViewCollection);
            _tabService = serviceProvider.GetService<ITabService>();
            Filters = new ModuleIndexFileFilter();
            _collectionViewSource = InitializeCollectionView(this.File.Entries);
        }

        private async Task FilterViewCollection()
        {
            RefreshList();
        }

        private ICollectionView InitializeCollectionView(ConcurrentBag<EntryRef> values)
        {
            
            var collectionView = CollectionViewSource.GetDefaultView(values);
            collectionView.Filter = OnFilterFiles;

            return collectionView;
        }

        private bool OnFilterFiles(object obj)
        {
            EntryRef entryRef = (EntryRef)obj;
            bool isValid = true;
            if (!string.IsNullOrEmpty(Filters.GlobalId) && !entryRef.globalId.ToString().Contains(Filters.GlobalId)) {
                isValid = false;
            }
            if (!string.IsNullOrEmpty(Filters.GlobalIdRefIn)) {
                bool refFound = false;
                foreach (var item in entryRef.subentry)
                {
                    foreach (var refers in item.references)
                    {
                        if (refers.globalId.ToString().Contains(Filters.GlobalIdRefIn)) {
                            refFound = true;
                            break;
                        }
                    }
                    if (refFound) {
                        break;
                    }
                }
                isValid = refFound; 
            }
            return isValid;
        }

        private async Task OpenGenFileTab(int arg)
        {
            IHIRTFile file = HIFileContext.Instance.GetFile(arg);
            if (file == null)
            {
                await ShowMessageModal(
                            title: "Ref to file not found.",
                            message: $"We can't open the global id {arg}.");
                return;
            }



            if (!_tabService.CreateTabForFile(file, out _, true))
            {
                
                await ShowMessageModal(
                  title: "Unsupported File Type",
                  message: $"We can't open {file.TagGroup} files yet.");

                return;
            }
        }

        protected override async Task OnInitializing()
        {
            var modal = ServiceProvider.GetService<ProgressModal>();
            //modal.DataContext = new ;

            using (modal)
            {
                Modals.Add(modal);
                modal.Show();
                IsBusy = true;
                await Task.Factory.StartNew(File.ReadEntrys, TaskCreationOptions.LongRunning);
                //File.ReadEntrys();

                RefreshList();

                await modal.Hide();
                Modals.Remove(modal);

                IsBusy = false;
            }

            
            await base.OnInitializing();
        }

        private void RefreshList()
          => App.Current.Dispatcher.Invoke(_collectionViewSource.Refresh);
    }
}
