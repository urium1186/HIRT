using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Processes;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.Common;
using PropertyChanged;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using System.Windows.Threading;

namespace HaloInfiniteResearchTools.Models
{
    public class FileContextModel : ObservableObject
    {
        #region Data Members

        private readonly HIFileContext _context;
        //private readonly ObservableCollection<IHIRTFile> _files;
        // private readonly ObservableCollection<DirModel> _dirFiles;
        //private readonly ObservableCollection<TreeHierarchicalModel> _filesH;

        private bool _isRefreshing = false;
        private object loockObject = new object();

        private IReadOnlySet<string> _editorFileExtensions;


        private readonly ICollectionView _collectionViewSource;
        private readonly ICollectionView _collectionDirsViewSource;

        private ActionThrottler _throttler;

        //private ActionThrottler _throttlerDir;
        // private ConcurrentQueue<DirModel> _dirsFileAddQueue;
        // private ConcurrentQueue<DirModel> _dirsFileRemoveQueue;
        // private ConcurrentDictionary<string, DirModel> _dirsFileLookup;

        private string _searchTerm = ""; //control_example_2.render_model

        #endregion

        #region Properties

        public ICollectionView Files
        {
            get => _collectionViewSource;
        }

        public ICollectionView DirFiles
        {
            get => _collectionDirsViewSource;
        }

        [OnChangedMethod(nameof(RefreshFileTree))]
        public bool ShowUnsupportedFiles { get; set; }

        public ICommand SearchTermChangedCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand OpenDirectoryCommand { get; }

        public ObservableCollection<DirModel> FiltersDirs { get; set; }
        public HIFileContext HiContext { get => _context as HIFileContext; }

        //public ObservableCollection<TreeHierarchicalModel> FilesH => _filesH;

        public ICollection<IHIRTFile> HIRTFiles { get => (_context as HIFileContext).Files; }

        #endregion

        #region Constructor

        public FileContextModel()
        {
            var serviceProvider = ((App)App.Current).ServiceProvider;

            // Initialize the underlying collection
            _context = HIFileContext.Instance;
            _context.FileAdded += OnFileAdded;
            _context.FileRemoved += OnFileRemoved;
            _context.InitializeThreadSynchronizationEvent += FileContextModel_InitializeThreadSynchronizationEvent;
            _collectionViewSource = InitializeCollectionView(_context.Files);
            _context.Init();
            HiContext.InitDbHashTable();
            
            _throttler = new ActionThrottler(UpdateFiles, 1500);
            
            SearchTermChangedCommand = new Command<string>(OnSearchTermUpdated);
        }

        private void FileContextModel_InitializeThreadSynchronizationEvent(object? sender, (ObservableCollection<IHIRTFile>, object) param)
        {
            InitializeThreadSynchronization(param.Item1, param.Item2);
        }

        #endregion

        public void fillMemFilesbyGroup()
        {

        }

        #region Overrides
        override protected void OnDisposing()
        {
            HiContext.Dispose();
        }

        public void reset()
        {
            UpdateFiles();
            HiContext.reset();
        }
        #endregion

        #region Private Methods
        private void InitializeThreadSynchronizationTuple(
         (ObservableCollection<IHIRTFile>, object) param)
        {
            InitializeThreadSynchronization(param.Item1, param.Item2);
        }
        private void InitializeThreadSynchronization(
          ICollection<IHIRTFile> files, object collectionLock)
        {
            // Initialize the underlying file collection with a lock so that we can
            // update the collection on other threads when the thread owns the lock.
            App.Current.Dispatcher.Invoke(() =>
            {
                BindingOperations.EnableCollectionSynchronization(files, collectionLock);
            });
        }
        private void InitializeThreadSynchronization(
          ObservableCollection<DirModel> files, object collectionLock)
        {
            // Initialize the underlying file collection with a lock so that we can
            // update the collection on other threads when the thread owns the lock.
            App.Current.Dispatcher.Invoke(() =>
            {
                BindingOperations.EnableCollectionSynchronization(files, collectionLock);
            });
        }
        private void InitializeThreadSynchronization(
          ObservableCollection<TreeHierarchicalModel> files, object collectionLock)
        {
            // Initialize the underlying file collection with a lock so that we can
            // update the collection on other threads when the thread owns the lock.
            App.Current.Dispatcher.Invoke(() =>
            {
                BindingOperations.EnableCollectionSynchronization(files, collectionLock);
            });
        }

        private ICollectionView InitializeCollectionView(IEnumerable<IHIRTFile> files)
        {
            var collectionView = CollectionViewSource.GetDefaultView(files);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(IHIRTFile.TagGroup)));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(IHIRTFile.TagGroup), ListSortDirection.Ascending));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(IHIRTFile.Name), ListSortDirection.Ascending));
            collectionView.Filter = OnFilterFiles;

            return collectionView;
        }
        private async void UpdateFilesAsync()
        {
            await Task.Run(new System.Action(UpdateFiles));
        }

        private void UpdateFiles()
        {
            lock (loockObject)
            {
                if (!_isRefreshing)
                {
                    _isRefreshing = true;
                    RefreshFileTree();
                    _isRefreshing = false;
                }

            }

        }
        #endregion

        #region Event Handlers

        private void OnFileAdded(object sender, IHIRTFile file)
        {
            //_throttler.Execute();
        }

        private void OnFileRemoved(object sender, IHIRTFile file)
        {
            //_throttler.Execute();
        }

        private bool OnFilterFiles(object obj)
        {
            var file = obj as IHIRTFile;
            if (!ShowUnsupportedFiles && _editorFileExtensions != null && !_editorFileExtensions.Contains(file.Extension))
                return false;

            if (file == null) return false;

            if (file.Extension == ".module") return false;

            if (!string.IsNullOrWhiteSpace(_searchTerm))
                return file.Name.Contains(_searchTerm, System.StringComparison.InvariantCultureIgnoreCase);

            return true;
        }

        private void OnSearchTermUpdated(string searchTerm)
        {
            _searchTerm = searchTerm;
            
            _throttler.Execute();
        }

        private void AddExistingFiles()
        {

            //_context.OpenModulesInDirPath("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Halo Infinite\\deploy\\");
            //return;
            /*foreach (var file in _context.Files.Values.ToArray())
                OnFileAdded(this, file);*/
        }

        private void RefreshFileTree()
          => App.Current.Dispatcher.Invoke(DispatcherPriority.Background, _collectionViewSource.Refresh);



        #endregion


    }
}
