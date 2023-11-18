using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files.FileTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(ModuleIncrFile))]
    public class ModuleIncrFileViewModel : SSpaceFileViewModel<ModuleIncrFile>, IDisposeWithView
    {
        private readonly ICollectionView _collectionViewSource;

        public ICollectionView Entries
        {
            get => _collectionViewSource;
        }
        public ModuleIncrFileViewModel(IServiceProvider serviceProvider, ModuleIncrFile file) : base(serviceProvider, file)
        {
            _collectionViewSource = InitializeCollectionView(this.File.Entries);
            
        }


        private ICollectionView InitializeCollectionView(ConcurrentQueue<IncrEntry> values)
        {

            var collectionView = CollectionViewSource.GetDefaultView(values);
            collectionView.Filter = OnFilterFiles;

            return collectionView;
        }

        private bool OnFilterFiles(object obj)
        {
            return true;
        }

        protected override Task OnInitializing()
        {
            File.ReadEntrys();
            RefreshList();
            return base.OnInitializing();
        }

        private void RefreshList()
         => App.Current.Dispatcher.Invoke(_collectionViewSource.Refresh);
    }
}
