using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Processes;
using LibHIRT.Files;
using LibHIRT.Files.Base;
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

namespace HaloInfiniteResearchTools.Models
{
    public class FileContextModel : ObservableObject
    {
        #region Data Members

        private readonly IHIFileContext _context;
        private readonly ObservableCollection<IHIRTFile> _files;
        // private readonly ObservableCollection<DirModel> _dirFiles;
        //private readonly ObservableCollection<TreeHierarchicalModel> _filesH;



        private IReadOnlySet<string> _editorFileExtensions;

        private readonly object _collectionLock;
        private readonly ICollectionView _collectionViewSource;
        private readonly ICollectionView _collectionDirsViewSource;

        private ActionThrottler _throttler;
        private ConcurrentQueue<IHIRTFile> _fileAddQueue;
        private ConcurrentQueue<IHIRTFile> _fileRemoveQueue;
        private ConcurrentDictionary<int, IHIRTFile> _fileLookup;
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


        #endregion

        #region Constructor

        public FileContextModel()
        {
            var serviceProvider = ((App)App.Current).ServiceProvider;

            // Initialize the underlying collection
            _collectionLock = new object();
            _files = new ObservableCollection<IHIRTFile>();
            //_dirFiles = new ObservableCollection<DirModel>();
            //_filesH = new ObservableCollection<TreeHierarchicalModel>();

            InitializeThreadSynchronization(_files, _collectionLock);
            //InitializeThreadSynchronization(_dirFiles, _collectionLock);
            //InitializeThreadSynchronization(_filesH, _collectionLock);

            // Initialize File Queues/Throttlers
            _throttler = new ActionThrottler(UpdateFilesAsync, 500);
            _fileAddQueue = new ConcurrentQueue<IHIRTFile>();
            _fileRemoveQueue = new ConcurrentQueue<IHIRTFile>();
            _fileLookup = new ConcurrentDictionary<int, IHIRTFile>();

            // Initialize File Queues/Throttlers
            //_throttlerDir = new ActionThrottler(UpdateDirsFiles, 1000);
            //_dirsFileAddQueue = new ConcurrentQueue<DirModel>();
            //_dirsFileRemoveQueue = new ConcurrentQueue<DirModel>();
            //_dirsFileLookup = new ConcurrentDictionary<string, DirModel>();

            // Initialize File Extension Lookup
            //var fileTypeService = serviceProvider.GetRequiredService<IFileTypeService>();
            //_editorFileExtensions = fileTypeService.ExtensionsWithEditorSupport;

            // Initialize the LibH2A Context
            _context = HIFileContext.Instance;
            _context.FileAdded += OnFileAdded;
            _context.FileRemoved += OnFileRemoved;
            HiContext.InitDbHashTable();
            // Initialize the collection view source
            _collectionViewSource = InitializeCollectionView(_files);
            //_collectionDirsViewSource = InitializeCollectionView(_dirFiles);

            // Initialize Commands
            SearchTermChangedCommand = new Command<string>(OnSearchTermUpdated);

            AddExistingFiles();

            //_context.OpenFile( @"G:\h2a\re files\dervish__h.tpl" );
            //foreach ( var file in Directory.GetFiles( @"G:\h2a\d\", "*.pct", SearchOption.AllDirectories ) )
            //  if ( file.Contains( "masterchief" ) )
            //    _context.OpenFile( file );
        }

        #endregion
        
        public void fillMemFilesbyGroup()
        {
            
            
            try
            {
                lock (_files)
                {
                    Dictionary<string, TreeHierarchicalModel> valuePairs = new Dictionary<string, TreeHierarchicalModel>();
                    _files.Clear();

                    var list = HIFileContext.RuntimeTagLoader.TagsList.Values.ToList();
                    //list.Sort((x, y) => x.TagGroup.CompareTo(y.TagGroup));
                    foreach (var item in list)
                    {
                       _files.Add(item);    
                    }
                }
                UpdateFiles();

            }
            catch (Exception ex)
            {

                throw ex;
            }
            

            /*
            var test = HIFileContext.RuntimeTagLoader.TagsList.Values.GroupBy(u => u._tagGroup);
            var r = test.ToDictionary(g => g.Key, g => g.ToList());
            r.Keys*/
        }
        /*private ObservableCollection<DirModel> filterList()
        {
            var temp = new ObservableCollection<DirModel>();
            if (this._searchTerm == "")
            {
                temp.Add(HIFileContext.RootDir);
                return temp;
            }

            try
            {
                var rootDir = filterList(HIFileContext.RootDir);
                temp.Add(rootDir);
                return temp;
            }
            catch (Exception ex)
            {


            }

            //return _context.RootDir;
            temp.Add(HIFileContext.RootDir);
            return temp;
        }*/

        private async Task OpenModuleEntryFile(FileModel file)
        {

            if (file == null)
                return;
            List<FileModel> l = new List<FileModel>();
            l.Add(file);
            var process = new OpenModuleEntryFileProcess(l);
            await RunProcess(process);
        }

        protected async Task RunProcess(IProcess process)
        {
            await Task.Factory.StartNew(process.Execute, TaskCreationOptions.LongRunning);
            await process.CompletionTask;

        }

        private DirModel? filterList(DirModel dir)
        {

            if (dir == null) return null;
            if (dir.GetType() == typeof(FileDirModel))
            {
                string searh_in = string.IsNullOrEmpty(((FileDirModel)dir).File.Path_string) ? ((FileDirModel)dir).File.Name : ((FileDirModel)dir).File.Path_string;
                if (searh_in.Contains(this._searchTerm))
                    return new FileDirModel(((FileDirModel)dir).File, dir.SubPath);
                return null;
            }
            else
            {
                DirModel temp = new DirModel(dir.SubPath);
                foreach (var item in dir.ListDirs)
                {
                    var t_r = filterList(item);
                    if (t_r != null)
                    {
                        temp.Dirs[item.SubPath] = t_r;
                    }
                }
                if (temp.Dirs.Count != 0)
                    return temp;

            }
            return null;
        }

        #region Overrides
        override protected void OnDisposing()
        {
            for (int i = 0; i < _files.Count; i++)
            {
                _files[i].reset();
            } 
            _files.Clear();
            //_dirFiles = new ObservableCollection<DirModel>();
           
            

            _fileAddQueue.Clear();
            _fileRemoveQueue.Clear();
            _fileLookup.Clear();
            
            //_context.FileAdded -= OnFileAdded;
            //_context.FileRemoved -= OnFileRemoved;
            
            HiContext.Dispose();
            //_throttlerDir.Execute();
            //_throttler.Dispose();
            //_throttlerDir.Dispose();
            // Initialize the collection view source

        }

        public void reset() {
            for (int i = 0; i < _files.Count; i++)
            {
                _files[i].reset();
            }
            _files.Clear();
            //_dirFiles = new ObservableCollection<DirModel>();



            _fileAddQueue.Clear();
            _fileRemoveQueue.Clear();
            _fileLookup.Clear();
            UpdateFiles();
            HiContext.reset();
        }
        #endregion

        #region Private Methods

        private void InitializeThreadSynchronization(
          ObservableCollection<IHIRTFile> files, object collectionLock)
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

        private ICollectionView InitializeCollectionView(ObservableCollection<IHIRTFile> files)
        {
            var collectionView = CollectionViewSource.GetDefaultView(files);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(IHIRTFile.TagGroup)));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(IHIRTFile.TagGroup), ListSortDirection.Ascending));
            //collectionView.SortDescriptions.Add(new SortDescription(nameof(IHIRTFile.Name), ListSortDirection.Ascending));
            collectionView.Filter = OnFilterFiles;

            return collectionView;
        }
        private ICollectionView InitializeCollectionView(ObservableCollection<DirModel> files)
        {
            var collectionView = CollectionViewSource.GetDefaultView(files);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DirModel.SubPath)));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(DirModel.SubPath), ListSortDirection.Ascending));
            //collectionView.SortDescriptions.Add(new SortDescription(nameof(FileModel.Name), ListSortDirection.Ascending));
            collectionView.Filter = OnFilterDirFiles;

            return collectionView;
        }
        private async void UpdateFilesAsync() {
            await Task.Run(new System.Action(UpdateFiles));
        }

        private void UpdateFiles()
        {
            
            lock (_collectionLock)
            {
                while (_fileAddQueue.TryDequeue(out var fileToAdd))
                    _files.Add(fileToAdd);

                while (_fileRemoveQueue.TryDequeue(out var fileToRemove))
                    _files.Remove(fileToRemove);
            }

            RefreshFileTree();
            if (_fileAddQueue.Count > 0 || _fileRemoveQueue.Count > 0)
                _throttler.Execute();
        }

        private void UpdateDirsFiles()
        {
            /*if (HIFileContext.RootDir != null)
                FiltersDirs = filterList();
            else
            {

            }

            
            lock (_collectionLock)
            {
                while (_dirsFileAddQueue.TryDequeue(out var fileToAdd))
                    _dirFiles.Add(fileToAdd);

                while (_dirsFileRemoveQueue.TryDequeue(out var fileToRemove))
                    _dirFiles.Remove(fileToRemove);
            }

            RefreshFileTree();
            if (_dirsFileAddQueue.Count > 0 || _dirsFileRemoveQueue.Count > 0)
                _throttlerDir.Execute();*/
        }

        #endregion

        #region Event Handlers

        private void OnFileAdded(object sender, IHIRTFile file)
        {
            // Explicitly exclude pck files from the FileTree


            if (_fileLookup.TryAdd(file.TryGetGlobalId(), file))
            {
                _fileAddQueue.Enqueue(file);
                _throttler.Execute();
            }
            /*if (_dirsFileLookup.TryAdd(file.Name, dirModel))
            {
                _dirsFileAddQueue.Enqueue(dirModel);
                _throttlerDir.Execute();
            }*/
        }

        private void OnFileRemoved(object sender, IHIRTFile file)
        {
            if (!_fileLookup.TryGetValue(file.TryGetGlobalId(), out var model))
                return;

            _fileRemoveQueue.Enqueue(model);
            _throttler.Execute();
            /*if (!_dirsFileLookup.TryGetValue(file.Name, out var dirModel))
                return;

            _dirsFileRemoveQueue.Enqueue(dirModel);*/
            //_throttlerDir.Execute();
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

        private bool OnFilterDirFiles(object obj)
        {
            var file = obj as DirModel;
            /*if (!ShowUnsupportedFiles && _editorFileExtensions != null && !_editorFileExtensions.Contains(file.Extension))
                return false;
            */
            if (!string.IsNullOrWhiteSpace(_searchTerm))
                return file.SubPath.Contains(_searchTerm, System.StringComparison.InvariantCultureIgnoreCase);

            return true;
        }

        private void OnSearchTermUpdated(string searchTerm)
        {
            _searchTerm = searchTerm;
            var grou = _collectionViewSource.Groups;
            _throttler.Execute();

            //_throttlerDir.Execute();
        }

        private void AddExistingFiles()
        {

            //_context.OpenModulesInDirPath("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Halo Infinite\\deploy\\");
            //return;
            foreach (var file in _context.Files.Values.ToArray())
                OnFileAdded(this, file);
        }

        private void RefreshFileTree()
          => App.Current.Dispatcher.Invoke(_collectionViewSource.Refresh);

        #endregion


    }
}
