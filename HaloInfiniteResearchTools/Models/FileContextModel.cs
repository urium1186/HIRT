using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Processes;
using LibHIRT.Files;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly ObservableCollection<FileModel> _files;
        // private readonly ObservableCollection<DirModel> _dirFiles;
        private readonly ObservableCollection<TreeHierarchicalModel> _filesH;
        


        private IReadOnlySet<string> _editorFileExtensions;

        private readonly object _collectionLock;
        private readonly ICollectionView _collectionViewSource;
        private readonly ICollectionView _collectionDirsViewSource;

        private ActionThrottler _throttler;
        private ConcurrentQueue<FileModel> _fileAddQueue;
        private ConcurrentQueue<FileModel> _fileRemoveQueue;
        private ConcurrentDictionary<string, FileModel> _fileLookup;
        private ActionThrottler _throttlerDir;
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

        public ObservableCollection<TreeHierarchicalModel> FilesH => _filesH;


        #endregion

        #region Constructor

        public FileContextModel()
        {
            var serviceProvider = ((App)App.Current).ServiceProvider;

            // Initialize the underlying collection
            _collectionLock = new object();
            _files = new ObservableCollection<FileModel>();
            //_dirFiles = new ObservableCollection<DirModel>();
            _filesH = new ObservableCollection<TreeHierarchicalModel>();

            InitializeThreadSynchronization(_files, _collectionLock);
            //InitializeThreadSynchronization(_dirFiles, _collectionLock);
            InitializeThreadSynchronization(_filesH, _collectionLock);

            // Initialize File Queues/Throttlers
            _throttler = new ActionThrottler(UpdateFiles, 1000);
            _fileAddQueue = new ConcurrentQueue<FileModel>();
            _fileRemoveQueue = new ConcurrentQueue<FileModel>();
            _fileLookup = new ConcurrentDictionary<string, FileModel>();

            // Initialize File Queues/Throttlers
            _throttlerDir = new ActionThrottler(UpdateDirsFiles, 1000);
            //_dirsFileAddQueue = new ConcurrentQueue<DirModel>();
            //_dirsFileRemoveQueue = new ConcurrentQueue<DirModel>();
            //_dirsFileLookup = new ConcurrentDictionary<string, DirModel>();

            // Initialize File Extension Lookup
            //var fileTypeService = serviceProvider.GetRequiredService<IFileTypeService>();
            //_editorFileExtensions = fileTypeService.ExtensionsWithEditorSupport;

            // Initialize the LibH2A Context
            _context = serviceProvider.GetRequiredService<IHIFileContext>();
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

        public void fillMemFilesbyGroup() {
            try
            {
                lock (_filesH)
                {
                    Dictionary<string, TreeHierarchicalModel> valuePairs = new Dictionary<string, TreeHierarchicalModel>();
                    _filesH.Clear();


                    foreach (var item in HIFileContext.RuntimeTagLoader.TagsList.Values)
                    {
                        if (!valuePairs.ContainsKey(item.TagGroup))
                        {
                            valuePairs[item.TagGroup] = new TreeHierarchicalModel();
                            valuePairs[item.TagGroup].Name = item.TagGroup;
                            valuePairs[item.TagGroup].Childrens = new List<TreeHierarchicalModel>();
                            _filesH.Add(valuePairs[item.TagGroup]);
                        }
                        var itemI = new TreeHierarchicalModel();
                        itemI.Name = item.TagFullName;
                        itemI.Value = item;
                        valuePairs[item.TagGroup].Childrens.Add(itemI);
                    }
                }
                
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
        private ObservableCollection<DirModel> filterList()
        {
            var temp = new ObservableCollection<DirModel>();
            if (this._searchTerm == "") {
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
        }

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

        protected override void OnDisposing()
        {
            _context.FileAdded -= OnFileAdded;
            _context.FileRemoved -= OnFileRemoved;
            _throttler.Dispose();
            _throttlerDir.Dispose();
        }

        #endregion

        #region Private Methods

        private void InitializeThreadSynchronization(
          ObservableCollection<FileModel> files, object collectionLock)
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

        private ICollectionView InitializeCollectionView(ObservableCollection<FileModel> files)
        {
            var collectionView = CollectionViewSource.GetDefaultView(_files);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FileModel.Group)));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(FileModel.Group), ListSortDirection.Ascending));
            collectionView.SortDescriptions.Add(new SortDescription(nameof(FileModel.Name), ListSortDirection.Ascending));
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
            FiltersDirs = filterList();

            /*
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

        private void OnFileAdded(object sender, ISSpaceFile file)
        {
            // Explicitly exclude pck files from the FileTree
            

            var model = new FileModel(file);
            if (_fileLookup.TryAdd(file.Name, model))
            {
                _fileAddQueue.Enqueue(model);
                _throttler.Execute();
            }

            var dirModel = new DirModel(file.Name);
            /*if (_dirsFileLookup.TryAdd(file.Name, dirModel))
            {
                _dirsFileAddQueue.Enqueue(dirModel);
                _throttlerDir.Execute();
            }*/
        }

        private void OnFileRemoved(object sender, ISSpaceFile file)
        {
            if (!_fileLookup.TryGetValue(file.Name, out var model))
                return;

            _fileRemoveQueue.Enqueue(model);
            _throttler.Execute();
            /*if (!_dirsFileLookup.TryGetValue(file.Name, out var dirModel))
                return;

            _dirsFileRemoveQueue.Enqueue(dirModel);*/
            _throttlerDir.Execute();
        }

        private bool OnFilterFiles(object obj)
        {
            var file = obj as FileModel;
            if (!ShowUnsupportedFiles && _editorFileExtensions != null && !_editorFileExtensions.Contains(file.Extension))
                return false;

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
            
           // _throttler.Execute();
            _throttlerDir.Execute();
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
