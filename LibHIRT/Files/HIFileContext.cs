using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using LibHIRT.TagReader.RuntimeViewer;
using System.Collections.Concurrent;
using System.Data.SQLite;

namespace LibHIRT.Files
{
    
    public class HIFileContext : IHIFileContext
    {
        #region Events

        public event EventHandler<ISSpaceFile> FileAdded;
        public event EventHandler<ISSpaceFile> FileRemoved;

        static HIFileContext _instance;

        #endregion

        #region Data Members

        public static IReadOnlyCollection<string> SupportedFileExtensions
        {
            get => SSpaceFileFactory.SupportedFileExtensions;
        }
        public static IReadOnlyCollection<string> NoSupportedFileExtensions
        {
            get => SSpaceFileFactory.NoSupportedFileExtensions;
        }

        private readonly SemaphoreSlim _fileLock;
        private ConcurrentDictionary<int, ISSpaceFile> _files;
        private ConcurrentBag<int> _filesModuleGlobalIdLockUp;
        static RuntimeTagLoader _runtimeTagLoader = new RuntimeTagLoader();
        private SQLiteConnection connectionDb;
        private bool disposedValue;

        
        #endregion

        #region Properties

        public IReadOnlyDictionary<int, ISSpaceFile> Files
        {
            get => _files;
        }
        

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
        public static RuntimeTagLoader RuntimeTagLoader { get => _runtimeTagLoader; }
        

        #endregion

        #region Constructor

        protected HIFileContext()
        {
            _fileLock = new SemaphoreSlim(1);
            _files = new ConcurrentDictionary<int, ISSpaceFile>();
            _filesModuleGlobalIdLockUp = new ConcurrentBag<int>();
            _runtimeTagLoader.Completed += _runtimeTagLoader_Completed;

            //SQLiteDriver.CreateTable(connectionDb);
            //SQLiteDriver.InsertMmh3LTU(connectionDb);
            //SQLiteDriver.ReadData(connectionDb);
        }

        public static HIFileContext Instance { get { 
                if (_instance == null)
                    _instance = new HIFileContext();
                return _instance;
            } }

        private void _runtimeTagLoader_Completed(object? sender, EventArgs e)
        {
            RuntimeLoadCompleted = true;
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

        public bool AddFile(ISSpaceFile file)
        {
            bool filesAdded = false;

            if (!_files.ContainsKey(file.TryGetGlobalId()))
            {
                _files.TryAdd(file.TryGetGlobalId(), file);
                filesAdded = true;

                FileAdded?.Invoke(this, file);
            }
            SSpaceFile temp = file as ModuleFile;

            if (temp != null)
            {
                if (!_filesModuleGlobalIdLockUp.Contains(temp.TryGetGlobalId()))
                    _filesModuleGlobalIdLockUp.Add(temp.TryGetGlobalId());
                else
                {
                }
            }
            
            foreach (var childFile in file.Children)
            {
                filesAdded |= AddFile(childFile);
            }

            return filesAdded;
        }
        public ISSpaceFile SearchById(int id) {
            ISSpaceFile tempFile;
            _files.TryGetValue(id, out tempFile);
            return tempFile;
        }
        public ISSpaceFile SearchInFileOld(ISSpaceFile file, int id)
        {

            if (file is ModuleFile)
            {

            }
            else
            {
                int global_id = file.TryGetGlobalId();
                ISSpaceFile tempFile;
                _files.TryGetValue(global_id, out tempFile);
                if (tempFile == file)
                {
                    return file;
                }
                
            }

            foreach (var childFile in file.Children)
            {
                var result = SearchInFileOld(childFile, id);
                if (result != null)
                    return result;
            }

            return null;
        }

        /*
        public void AddFileToDirList(ISSpaceFile file)
        {
            if (!(file.GetType().IsSubclassOf(typeof(SSpaceFile))) || (file.GetType().IsSubclassOf(typeof(SSpaceContainerFile))))
            {
                return;
            }
            lock (_rootDir)
            {

                var dirSplit = string.IsNullOrEmpty(file.Path_string) ? file.Name.Split(@"\") : file.Path_string.Split(@"\");
                var tempDir = _rootDir;
                lock (tempDir)
                {
                    for (int i = 0; i < dirSplit.Length - 1; i++)
                    {
                        if (!tempDir.Dirs.ContainsKey(dirSplit[i]))
                        {
                            tempDir.Dirs[dirSplit[i]] = new DirModel(dirSplit[i]);
                            tempDir.Dirs[dirSplit[i]].Parent = tempDir;
                        }

                        tempDir = tempDir.Dirs[dirSplit[i]];
                    }
                    tempDir.Dirs[dirSplit[dirSplit.Length - 1]] = new FileDirModel(file, dirSplit[dirSplit.Length - 1]);
                    tempDir.Dirs[dirSplit[dirSplit.Length - 1]].Parent = tempDir;
                }


            }

        }
        */

        public ISSpaceFile GetFile(int global_id)
        {
            _files.TryGetValue(global_id, out var file);
            return file;
        }

        public TFile GetFile<TFile>(int global_id)
          where TFile : class, ISSpaceFile
          => GetFile(global_id) as TFile;

        public IEnumerable<TFile> GetFiles<TFile>()
          where TFile : class, ISSpaceFile
          => _files.Values.OfType<TFile>();

        public IEnumerable<ISSpaceFile> GetFilesOnNames(string searchPattern)
        {
            searchPattern = searchPattern.ToLower();

            foreach (var file in _files.Values)
                if (file.Name.ToLower().Contains(searchPattern))
                    yield return file;
        }

        public IEnumerable<ISSpaceFile> GetFiles(string searchPattern)
        {
            searchPattern = searchPattern.ToLower();

            foreach (var file in _files.Values)
            {
                string in_S = file.Path_string == null ? file.InDiskPath : file.Path_string;
                if (in_S.ToLower().Contains(searchPattern))
                    yield return file;
            }

        }

        public IEnumerable<TFile> GetFiles<TFile>(string searchPattern)
          where TFile : class, ISSpaceFile
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
                stream = HIRTDecompressionStream.FromFile(filePath);
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
                }
                file.InDiskPath = filePath;
                return AddFile(file);
            }
            finally { stream.ReleaseLock(); }
        }

        public bool RemoveFile(ISSpaceFile file)
        {
            if (_files.TryRemove(file.TryGetGlobalId(), out _))
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

        public ISSpaceFile GetFileFrom(TagRef tagRef, ModuleFile init = null)
        {
            if (tagRef == null)
                return null;
            ISSpaceFile result = null;
            if (init != null)
            {

                result = init.GetFileByGlobalId(tagRef.Ref_id_int);
            }
            if (result == null)
            {
                _files.TryGetValue(tagRef.Ref_id_int, out result);
            }

            return result;
        }

        public ISSpaceFile GetFileFrom(int globalId, ModuleFile init = null)
        {
            ISSpaceFile result = null;
            if (init != null)
            {

                result = init.GetFileByGlobalId(globalId);
            }
            if (result == null)
            {
                _files.TryGetValue(globalId, out result);
            }

            return result;
        }


        public static TagStructMemFile GetMemFileFrom(TagRef tagRef)
        {
            if (tagRef == null)
                return null;
            TagStructMemFile result = null;
            _runtimeTagLoader.TagsList.TryGetValue(tagRef.Ref_id_int, out result);
            return result;
        }

        public ISSpaceFile OpenFileWithIdInModule(string modulePath, int id, bool load_resource = false)
        {
            if (_files.ContainsKey(id))
                return _files[id];

            var fileExt = Path.GetExtension(modulePath);
            if (!File.Exists(modulePath))
                return null;

            HIRTStream stream;
            if (fileExt == ".module")
            {
                if (modulePath.Contains("\\ds\\"))
                    return null;
                stream = HIRTDecompressionStream.FromFile(modulePath);
            }
            else
                return null;

            try
            {
                stream.AcquireLock();
                var file = SSpaceFileFactory.CreateFile(modulePath, "");
                if (fileExt == ".module")
                {
                    (file as ModuleFile).InitializeStream(stream, 0, stream.Length);
                }
                if (file is null)
                    return null;
                file.InDiskPath = modulePath;
                return SearchInFileOld(file, id);

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
                item.Value.reset();
                item.Value.Dispose();
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
                    var count = _files.Keys.Count;
                    foreach (var item in _files)
                    {
                        item.Value.Dispose();
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
