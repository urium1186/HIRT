using LibHIRT.Files.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Files
{
    public interface IHIFileContext : IDisposable
    {

        #region Events

        event EventHandler<IHIRTFile> FileAdded;
        event EventHandler<IHIRTFile> FileRemoved;

        #endregion

        #region Properties

        ObservableCollection<IHIRTFile> Files { get; }

        public string TagTemplatePath { get; set; }

        #endregion

        #region Public Methods

        bool AddFile(IHIRTFile file);
        bool RemoveFile(IHIRTFile file);
        IHIRTFile GetFile(int fileName);
        TFile GetFile<TFile>(int fileName) where TFile : class, IHIRTFile;

        IEnumerable<IHIRTFile> GetFiles(string searchPattern);
        IEnumerable<TFile> GetFiles<TFile>(string searchPattern) where TFile : class, IHIRTFile;
        IEnumerable<TFile> GetFiles<TFile>() where TFile : class, IHIRTFile;

        bool OpenDirectory(string path);
        bool OpenFile(string filePath);
        Task<bool> OpenFromRuntime(string filePath);
        IHIRTFile OpenFileWithIdInModule(string modulePath, int id, bool load_resource);

        #endregion

    }


}
