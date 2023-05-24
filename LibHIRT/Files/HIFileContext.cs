using LibHIRT.Files.Base;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using LibHIRT.TagReader.Common;
using LibHIRT.TagReader.RuntimeViewer;
using System.Collections.Concurrent;
using System.Data.SQLite;
using static LibHIRT.TagReader.TagLayouts;

namespace LibHIRT.Files
{
    public interface IHIFileContext
    {

        #region Events

        event EventHandler<ISSpaceFile> FileAdded;
        event EventHandler<ISSpaceFile> FileRemoved;

        #endregion

        #region Properties

        IReadOnlyDictionary<string, ISSpaceFile> Files { get; }
        static DirModel RootDir { get; }

        public string TagTemplatePath { get; set; }

        #endregion

        #region Public Methods

        bool AddFile(ISSpaceFile file);
        bool RemoveFile(ISSpaceFile file);
        bool ReadTagOnFile(ISSpaceFile file);
        ISSpaceFile GetFile(string fileName);
        TFile GetFile<TFile>(string fileName) where TFile : class, ISSpaceFile;

        IEnumerable<ISSpaceFile> GetFiles(string searchPattern);
        IEnumerable<TFile> GetFiles<TFile>(string searchPattern) where TFile : class, ISSpaceFile;
        IEnumerable<TFile> GetFiles<TFile>() where TFile : class, ISSpaceFile;

        bool OpenDirectory(string path);
        bool OpenFile(string filePath);
        bool OpenFromRuntime(string filePath);

        #endregion

    }


    public class HIFileContext : IHIFileContext
    {
        static HIFileContext Instance { get; set; }

        #region Events

        public event EventHandler<ISSpaceFile> FileAdded;
        public event EventHandler<ISSpaceFile> FileRemoved;

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
        private ConcurrentDictionary<string, ISSpaceFile> _files;
        private static ConcurrentDictionary<string, ModuleFile> _filesModuleGlobalIdLockUp;
        private static ConcurrentDictionary<int, ISSpaceFile> _filesGlobalIdLockUp;
        private static DirModel _rootDir = new DirModel("Root");
        private string _selectedJsonstring;
        static RuntimeTagLoader _runtimeTagLoader = new RuntimeTagLoader();
        private SQLiteConnection connectionDb;

        public string SelectedJsonstring { get => _selectedJsonstring; set => _selectedJsonstring = value; }
        #endregion

        #region Properties

        public IReadOnlyDictionary<string, ISSpaceFile> Files
        {
            get => _files;
        }

        public static ConcurrentDictionary<string, ModuleFile> FilesModuleGlobalIdLockUp { get => _filesModuleGlobalIdLockUp; }
        public static DirModel RootDir => _rootDir;



        public string TagTemplatePath
        {
            get
            {
                return TagXmlParse.TagsPath;
            }
            set
            {
                TagXmlParse.TagsPath = value;
            }
        }

        public bool RuntimeLoadCompleted { get; private set; }
        public static RuntimeTagLoader RuntimeTagLoader { get => _runtimeTagLoader; }
        public static ConcurrentDictionary<int, ISSpaceFile> FilesGlobalIdLockUp { get => _filesGlobalIdLockUp; set => _filesGlobalIdLockUp = value; }





        #endregion

        #region Constructor

        public HIFileContext()
        {
            _fileLock = new SemaphoreSlim(1);
            _files = new ConcurrentDictionary<string, ISSpaceFile>();
            _filesModuleGlobalIdLockUp = new ConcurrentDictionary<string, ModuleFile>();
            _filesGlobalIdLockUp = new ConcurrentDictionary<int, ISSpaceFile>();
            _runtimeTagLoader.Completed += _runtimeTagLoader_Completed;

            //SQLiteDriver.CreateTable(connectionDb);
            //SQLiteDriver.InsertMmh3LTU(connectionDb);
            //SQLiteDriver.ReadData(connectionDb);
        }

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

            if (!_files.ContainsKey(file.Name))
            {
                _files.TryAdd(file.Name, file);
                filesAdded = true;
                AddFileToDirList(file);

                FileAdded?.Invoke(this, file);
            }
            SSpaceFile temp = file as ModuleFile;

            if (temp != null)
            {
                if (!_filesModuleGlobalIdLockUp.ContainsKey(temp.Name))
                    _filesModuleGlobalIdLockUp.TryAdd(temp.Name, (ModuleFile)file);
            }
            else
            {
                int global_id = file.TryGetGlobalId();
                var temp_s = Mmr3HashLTU.getMmr3HashFromInt(global_id);
                if (global_id != -1)
                    _filesGlobalIdLockUp.TryAdd(global_id, file);
                else
                {
                }

            }

            foreach (var childFile in file.Children)
            {
                filesAdded |= AddFile(childFile);
                try
                {
                    AddFileToDirList(childFile);
                }
                catch (Exception e)
                {

                    throw e;
                }
            }

            return filesAdded;
        }

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

        public ISSpaceFile GetFile(string fileName)
        {
            fileName = fileName.ToLower();

            _files.TryGetValue(fileName, out var file);
            return file;
        }

        public TFile GetFile<TFile>(string fileName)
          where TFile : class, ISSpaceFile
          => GetFile(fileName) as TFile;

        public IEnumerable<TFile> GetFiles<TFile>()
          where TFile : class, ISSpaceFile
          => _files.Values.OfType<TFile>();

        public IEnumerable<ISSpaceFile> GetFiles(string searchPattern)
        {
            searchPattern = searchPattern.ToLower();

            foreach (var file in _files.Values)
                if (file.Name.ToLower().Contains(searchPattern))
                    yield return file;
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
            Parallel.ForEach(files, file => OpenFile(file));

            return filesAdded;
        }

        public bool OpenFile(string filePath)
        {
            var fileName = filePath.GetHashCode() + "__" + Path.GetFileName(filePath);
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
                var file = SSpaceFileFactory.CreateFile(fileName, stream, 0, stream.Length, "");
                if (file is null)
                    return false;
                file.InDiskPath = filePath;
                return AddFile(file);
            }
            finally { stream.ReleaseLock(); }
        }

        public bool ReadTagOnFile(ISSpaceFile file)
        {
            var tagParse = new TagParseControl(file.Path_string, file.TagGroup, null, file.GetStream());
            tagParse.readFile();
            _selectedJsonstring = tagParse.RootTagInst.ToJson();
            return true;
        }
        public bool RemoveFile(ISSpaceFile file)
        {
            if (_files.TryRemove(file.Name, out _))
            {
                FileRemoved?.Invoke(this, file);
                return true;
            }

            return false;
        }

        public bool OpenFromRuntime(string filePath)
        {
            RuntimeLoadCompleted = false;
            _runtimeTagLoader.HookAndLoad();
            return true;
        }

        public static ISSpaceFile GetFileFrom(TagRef tagRef, ModuleFile init = null)
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
                FilesGlobalIdLockUp.TryGetValue(tagRef.Ref_id_int, out result);
            }

            return result;
        }

        public static TagStructMem GetMemFileFrom(TagRef tagRef)
        {
            if (tagRef == null)
                return null;
            TagStructMem result = null;
            _runtimeTagLoader.TagsList.TryGetValue(tagRef.Ref_id, out result);
            return result;
        }

        #endregion

    }
}
