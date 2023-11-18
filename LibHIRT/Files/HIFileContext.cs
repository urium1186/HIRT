using LibHIRT.Common;
using LibHIRT.Files.Base;
using LibHIRT.Files.FileTypes;
using LibHIRT.Grunt;
using LibHIRT.TagReader;
using LibHIRT.TagReader.RuntimeViewer;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;

namespace LibHIRT.Files
{
    
    public class HIFileContext : IHIFileContext
    {
        #region Events

        public event EventHandler<IHIRTFile> FileAdded;
        public event EventHandler<IHIRTFile> FileRemoved;
        public event EventHandler<(ObservableCollection<IHIRTFile>, object)> InitializeThreadSynchronizationEvent;

        static HIFileContext _instance;

        #endregion

        #region Data Members

        public ObservableCollection<IHIRTFile> Files { get => _files; }

        public static IReadOnlyCollection<string> SupportedFileExtensions
        {
            get => SSpaceFileFactory.SupportedFileExtensions;
        }
        public static IReadOnlyCollection<string> NoSupportedFileExtensions
        {
            get => SSpaceFileFactory.NoSupportedFileExtensions;
        }

        private readonly SemaphoreSlim _fileLock;
        private ActionThrottler _throttler;
        private ConcurrentQueue<IHIRTFile> _fileAddQueue;
        private ConcurrentQueue<IHIRTFile> _fileRemoveQueue;

        private ConnectXboxServicesResult _connectXbox = null;


        private readonly object _collectionLock;
        ObservableCollection<IHIRTFile> _files;

        //private ConcurrentDictionary<int, IHIRTFile> _fileLookup;
        private ConcurrentBag<int> _filesModuleGlobalIdLockUp;
        private RuntimeTagLoader _runtimeTagLoader = new RuntimeTagLoader();
        private SQLiteConnection connectionDb;
        private bool disposedValue;

        
        #endregion

        #region Properties

        public string TagTemplatePath
        {
            get
            {
                return TagXmlParse.TagsPath;
            }
            set
            {
                TagXmlParse.TagsPath = value;
                TagXmlParseV2.TagsPath = value;
            }
        }

        public bool RuntimeLoadCompleted { get; protected set; }
        public RuntimeTagLoader RuntimeTagLoader { get => _runtimeTagLoader; }
        

        #endregion

        #region Constructor

        protected HIFileContext()
        {
            _fileLock = new SemaphoreSlim(1);
            _throttler = new ActionThrottler(UpdateFilesAsync, 1000);
            _fileAddQueue = new ConcurrentQueue<IHIRTFile>();
            _fileRemoveQueue = new ConcurrentQueue<IHIRTFile>();
            _filesModuleGlobalIdLockUp = new ConcurrentBag<int>();
            _runtimeTagLoader.Completed += _runtimeTagLoader_Completed;
            _collectionLock = new object();
            _files = new ObservableCollection<IHIRTFile>();
            //InitializeThreadSynchronization(_files, _collectionLock);
            //SQLiteDriver.CreateTable(connectionDb);
            //SQLiteDriver.InsertMmh3LTU(connectionDb);
            //SQLiteDriver.ReadData(connectionDb);
        }

        public void Init() {
            InitializeThreadSynchronizationEvent?.Invoke(this, (Files, _collectionLock));
        }
        public static HIFileContext Instance { get { 
                if (_instance == null)
                    _instance = new HIFileContext();
                return _instance;
            } }

        public ConnectXboxServicesResult ConnectXbox { get => _connectXbox; set => _connectXbox = value; }

        private void _runtimeTagLoader_Completed(object? sender, EventArgs e)
        {
            
            var list = RuntimeTagLoader.TagsList.Values.ToList();
            //list.Sort((x, y) => x.TagGroup.CompareTo(y.TagGroup));
            foreach (var item in list)
            {
                _files.Add(item);
            }
            _throttler.Execute();
            RuntimeLoadCompleted = true;

        }

        private void InitializeThreadSynchronization(
         ObservableCollection<IHIRTFile> files, object collectionLock)
        {
            // Initialize the underlying file collection with a lock so that we can
            // update the collection on other threads when the thread owns the lock.
            /*App.Current.Dispatcher.Invoke(() =>
            {
                BindingOperations.EnableCollectionSynchronization(files, collectionLock);
            });*/
        }

        private async void UpdateFilesAsync()
        {
            await Task.Run(new System.Action(UpdateFiles));
        }

        private void UpdateFiles()
        {

            lock (_collectionLock)
            {
                while (_fileAddQueue.TryDequeue(out var fileToAdd)) {
                    /*int id = fileToAdd.TryGetGlobalId();
                    if (id == -1)
                    {
                        id = fileToAdd.Name.GetHashCode();
                    }
                    if (_fileLookup.TryAdd(id, fileToAdd)) {
                        FileAdded?.Invoke(this, fileToAdd);
                    }*/
                    _files.Add(fileToAdd);
                    FileAdded?.Invoke(this, fileToAdd);
                }


                while (_fileRemoveQueue.TryDequeue(out var fileToRemove)) {
                    _files.Remove(fileToRemove);
                    FileRemoved?.Invoke(this, fileToRemove);
                    /*int id = fileToRemove.TryGetGlobalId();
                    if (id == -1)
                    {
                        id = fileToRemove.Name.GetHashCode();
                    }
                    if (_fileLookup.TryRemove(new KeyValuePair<int, IHIRTFile>(id, fileToRemove)))
                    {
                        FileRemoved?.Invoke(this, fileToRemove);
                    } */
                }
                    
            }

            if (_fileAddQueue.Count > 0 || _fileRemoveQueue.Count > 0)
                _throttler.Execute();
        }

        #endregion

        #region Public Methods

        public void InitDbHashTable()
        {
            Mmr3HashLTU.loadFromDbLtu();
            /*
            if (connectionDb == null)
                connectionDb = SQLiteDriver.CreateConnection();
            foreach (var item in Mmr3HashLTU.Mmr3lTU)
            {
                try
                {
                    SQLiteDriver.InsertMmh3LTU(connectionDb, item.Key, item.Value);
                }
                catch (Exception ex)
                {

                }
            }
            return; 
            using (StreamReader r = new StreamReader("in_use.json"))
            {
                string json = r.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                
                foreach (var item in array)
                {
                    try
                    {
                        Mmr3HashLTU.AddUniqueStrValue(item.Value.Value);
                        //SQLiteDriver.InsertMmh3LTU(connectionDb, Mmr3HashLTU.fromStrHash(item.Name), item.Value.Value, true, true);
                    }
                    catch (Exception ex)
                    {
                
                    }

                    Console.WriteLine("{0} {1}", item.Name, item.Value.Value);
                }
            }
           
            */

        }

        public bool AddFile(IHIRTFile file)
        {
            _fileAddQueue.Enqueue(file);
            
            bool filesAdded = false;
           
            SSpaceFile temp = file as ModuleFile;

            if (temp != null)
            {
                if (!_filesModuleGlobalIdLockUp.Contains(temp.TryGetGlobalId())) {
                    _filesModuleGlobalIdLockUp.Add(temp.TryGetGlobalId());
                    filesAdded = true;
                }
                
                else
                {
                }
            }
            if (file is SSpaceFile) {
                foreach (var childFile in (file as SSpaceFile).Children)
                {
                    filesAdded |= AddFile(childFile);
                }
            }

            _throttler.Execute();
            return filesAdded;
        }
      
        public IHIRTFile GetFile(int global_id)
        {
            foreach (var item in _files)
            {
                if (item.TryGetGlobalId() == global_id)
                    return item;
            }
            return null;
        }

        public TFile GetFile<TFile>(int global_id)
          where TFile : class, IHIRTFile
          => GetFile(global_id) as TFile;

        public IEnumerable<TFile> GetFiles<TFile>()
          where TFile : class, IHIRTFile
          => _files.OfType<TFile>();

        public IEnumerable<IHIRTFile> GetFilesOnNames(string searchPattern)
        {
            searchPattern = searchPattern.ToLower();

            foreach (var file in _files)
                if (file.Name.ToLower().Contains(searchPattern))
                    yield return file;
        }

        public IEnumerable<IHIRTFile> GetFiles(string searchPattern)
        {
            searchPattern = searchPattern.ToLower();

            foreach (IHIRTFile file in _files)
            {
                string in_S = file.Path_string == null ? file.InDiskPath : file.Path_string;
                if (in_S.ToLower().Contains(searchPattern))
                    yield return file;
            }

        }

        public IEnumerable<TFile> GetFiles<TFile>(string searchPattern)
          where TFile : class, IHIRTFile
          => GetFiles(searchPattern).OfType<TFile>();

        public bool OpenDirectory(string path)
        {
            var filesAdded = false;
            var files = Directory.EnumerateFiles(path, "*.module", SearchOption.AllDirectories);
            //foreach ( var file in files )
            //  filesAdded |= OpenFile( file );
            Parallel.ForEach(files, file => filesAdded |= OpenFile(file));

            return filesAdded;
        }

        public bool OpenFile(string filePath)
        {
            //var fileName = filePath.GetHashCode()+"__" + Path.GetFileName(filePath);
            var fileName = Path.GetFileName(filePath);
            var fileExt = Path.GetExtension(fileName);

            HIRTStream stream;
            if (fileExt == ".module")
            {
                if (filePath.Contains("\\ds\\"))
                    return false;
                stream = HIRTExtractedFileStream.FromFile(filePath);
            }
            else
                stream = HIRTExtractedFileStream.FromFile(filePath);

            try
            {
                stream.AcquireLock();
                var file = SSpaceFileFactory.CreateFile(fileName, "");
                if (file is null)
                    return false;
                if (fileExt == ".module")
                {
                    (file as ModuleFile).InitializeStream(stream, 0, stream.Length);
                    (file as ModuleFile).ReInitialize();
                }
                file.InDiskPath = filePath;
                return AddFile(file);
            }
            finally { stream.ReleaseLock(); }
        }

        public bool RemoveFile(IHIRTFile file)
        {
            if (_files.Remove(file))
            {
                FileRemoved?.Invoke(this, file);
                return true;
            }

            return false;
        }

        public  async Task<bool> OpenFromRuntime(string filePath)
        {
            RuntimeLoadCompleted = false;
            bool result = await _runtimeTagLoader.HookAndLoad();
            return true;
        }

        public List<EntryRef> getAllTagReferenceTo(int globalid) { 
            List<EntryRef> result = new List<EntryRef>();
            IEnumerable<ModuleIndexFile> indexRef = GetFiles<ModuleIndexFile>();
            foreach (ModuleIndexFile file in indexRef) {
                result.AddRange(file.getAllRefTo(globalid));
            }
            return result;
        }

        public IHIRTFile GetFileFrom(TagRef tagRef, ModuleFile init = null)
        {
            if (tagRef == null)
                return null;
            IHIRTFile result = null;
            if (init != null)
            {

                result = init.GetFileByGlobalId(tagRef.Ref_id_int);
            }
            if (result == null)
            {
                result = GetFile(tagRef.Ref_id_int);
            }

            return result;
        }

        public IHIRTFile OpenFileWithIdInModule(string modulePath, int id, bool load_resource = false)
        {
            var result = GetFile(id);
            if (result != null)
                return result;

            var fileExt = Path.GetExtension(modulePath);
            if (!File.Exists(modulePath))
                return null;

            HIRTStream stream;
            if (fileExt == ".module")
            {
                if (modulePath.Contains("\\ds\\"))
                    return null;
                stream = HIRTExtractedFileStream.FromFile(modulePath);
            }
            else
                return null;

            try
            {
                stream.AcquireLock();
                ModuleFile file = SSpaceFileFactory.CreateFile(modulePath, "") as ModuleFile;
                if (file is null)
                    return null;

                if (fileExt == ".module")
                {
                    file.InitializeStream(stream, 0, stream.Length);
                    file.ReInitialize();

                    file.InDiskPath = modulePath;

                    return file.GetFileByGlobalId(id);
                }

                return null;
            }
            finally { stream.ReleaseLock(); }
        }

        public void reset()
        {
            if (connectionDb != null)
                connectionDb.Dispose();

            foreach (var item in _filesModuleGlobalIdLockUp)
            {
                _files[item].reset();
            } 
            _filesModuleGlobalIdLockUp.Clear();
            
            foreach (var item in _files)
            {
                item.reset();
            }
            _files.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (connectionDb!=null)
                        connectionDb.Dispose();
                    foreach (var item in _files)
                    {
                        item.Dispose();
                    }
                    _files.Clear();

                    _filesModuleGlobalIdLockUp.Clear();

                }
                
                this._fileLock.Dispose();
                
                
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HIFileContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
           // GC.SuppressFinalize(this);
        }

        #endregion

    }
}
