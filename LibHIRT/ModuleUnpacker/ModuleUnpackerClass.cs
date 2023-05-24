namespace LibHIRT.ModuleUnpacker
{
    public class ModuleUnpackerClass
    {
        /*static public Dictionary<string, List<FileDirModel>> tagGroupFileList = new Dictionary<string, List<FileDirModel>>();
        static public Dictionary<int, List<FileDirModel>> idFileList = new Dictionary<int, List<FileDirModel>>();
        static private DirModel? _rootDir;

        public static DirModel? RootDir { get => _rootDir;}*/

        public static HiModule ReadModule(FileStream fileStream)
        {
            HiModule module = new HiModule();
            module.ReadIn(new BinaryReader(fileStream));

            return module;
        }
        public static HiModule ReadModule(string filePath)
        {
            HiModule module = new HiModule();
            module.ReadIn(filePath);

            return module;
        }

        public static void AddFileToDirList(HiModuleFileEntry fileEntry)
        {
            var dirSplit = fileEntry.Path_string.Split(@"\");
            /*if (_rootDir == null) {
                _rootDir = new DirModel("Root"); 
            }
            var tempDir = _rootDir;
            for (int i = 0; i < dirSplit.Length-1; i++)
            {
                if (!tempDir.Dirs.ContainsKey(dirSplit[i]))
                {
                    tempDir.Dirs[dirSplit[i]] = new DirModel(dirSplit[i]);
                    tempDir.Dirs[dirSplit[i]].Parent = tempDir; 
                }

                tempDir = tempDir.Dirs[dirSplit[i]];
            }
            tempDir.Dirs[dirSplit[dirSplit.Length - 1]] = new FileDirModel(_fileEntry, dirSplit[dirSplit.Length - 1]);
            tempDir.Dirs[dirSplit[dirSplit.Length - 1]].Parent= tempDir;    
            */
        }

    }
}
