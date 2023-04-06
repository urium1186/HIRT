using LibHIRT.ModuleUnpacker;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace LibHIRT.Files
{
    public class DirModel
    {
        protected string _path = "";
        Dictionary<string, DirModel> dirs = new Dictionary<string, DirModel>();
        DirModel? parent;

        public DirModel(string path)
        {
            this._path = path;
        }

        virtual public List<DirModel> ListDirs { get => arrangeDirs(); }

        virtual public Dictionary<string, DirModel>? Dirs { get => dirs; }
        virtual public string SubPath { get => _path; }

        
        public DirModel? Parent { get => parent; set => parent = value; }

        virtual public List<DirModel> FilterListDirs { get => getFilterList(); }

        private List<DirModel> arrangeDirs() { 
            var result = dirs.Values.ToList<DirModel>();
            result.Sort((x, y) => x.SubPath.CompareTo(y.SubPath)); 
            return result;
        }
        virtual public DirModel GetChildByPath(string path) {
            if (!string.IsNullOrEmpty(path)) {
                var a_split = path.Split('\\');
                if (a_split.Length == 1)
                {
                    if (a_split[0] == SubPath)
                        return this;
                    else if (dirs.ContainsKey(a_split[0]))
                        return dirs[a_split[0]].GetChildByPath(a_split[0]);
                }
                else {
                    if (dirs.ContainsKey(a_split[0])) {
                        string sub_path = string.Join('\\', a_split.Skip(1));
                        return dirs[a_split[0]].GetChildByPath(sub_path);
                    }
                }
                    
            }
            return null;
        }
        virtual protected List<DirModel> getFilterList() {
            var temp = filterDirs();
            if (temp == null || temp.Dirs == null)
                return new List<DirModel>();
            return temp.Dirs.Values.ToList<DirModel>();
        }
        virtual protected DirModel? filterDirs() {
            DirModel temp = new DirModel(SubPath);
            foreach (var item in ListDirs)
            {
                var t_r = item.filterDirs();
                if (t_r != null)
                {
                    temp.Dirs[item.SubPath] = t_r;
                }
            }
            if (temp.Dirs.Count != 0)
                return temp;
            return null;   
        }
    }

    public class FileDirModel : DirModel
    {
        ISSpaceFile _fileEntry;

        #region Properties

        public string Name
        {
            get => System.IO.Path.GetFileName(_fileEntry.Path_string);
        }

        public string Extension
        {
            get => System.IO.Path.GetExtension(_fileEntry.Path_string);
        }

        public string Group
        {
            get => _fileEntry.TagGroup;
        }

        public ISSpaceFile File
        {
            get => _fileEntry;
        }

        #endregion

        public FileDirModel(ISSpaceFile pFileEntry, string path) : base(path)
        {
            this._fileEntry = pFileEntry;    
        }

        public override Dictionary<string, DirModel>? Dirs => null;

        public override string SubPath => string.IsNullOrEmpty(_fileEntry.Path_string) ?_fileEntry.Name : System.IO.Path.GetFileName(_fileEntry.Path_string);

        public override List<DirModel> FilterListDirs => base.FilterListDirs;

        protected override DirModel? filterDirs()
        {
            return null;
        }

        public override DirModel GetChildByPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (path == SubPath)
                    return this;

            }
            return null;
        }
    }
}
