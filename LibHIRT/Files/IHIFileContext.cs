using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Files
{
    public interface IHIFileContext : IDisposable
    {

        #region Events

        event EventHandler<ISSpaceFile> FileAdded;
        event EventHandler<ISSpaceFile> FileRemoved;

        #endregion

        #region Properties

        IReadOnlyDictionary<int, ISSpaceFile> Files { get; }

        public string TagTemplatePath { get; set; }

        #endregion

        #region Public Methods

        bool AddFile(ISSpaceFile file);
        bool RemoveFile(ISSpaceFile file);
        ISSpaceFile GetFile(int fileName);
        TFile GetFile<TFile>(int fileName) where TFile : class, ISSpaceFile;

        IEnumerable<ISSpaceFile> GetFiles(string searchPattern);
        IEnumerable<TFile> GetFiles<TFile>(string searchPattern) where TFile : class, ISSpaceFile;
        IEnumerable<TFile> GetFiles<TFile>() where TFile : class, ISSpaceFile;

        bool OpenDirectory(string path);
        bool OpenFile(string filePath);
        Task<bool> OpenFromRuntime(string filePath);
        ISSpaceFile OpenFileWithIdInModule(string modulePath, int id, bool load_resource);

        #endregion

    }


}
