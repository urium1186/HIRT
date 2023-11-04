using LibHIRT.Files;
using LibHIRT.Files.Base;
using System;

namespace HaloInfiniteResearchTools.Models
{
    public interface IFileModel: IDisposable
    {
    }
    public class FileModel : IFileModel
    {

        #region Data Members

        private readonly IHIRTFile _file;

        #endregion

        #region Properties

        private bool _genericView = false;
        private bool disposedValue;

        public string Name
        {
            get => _file.Name;
        }

        public string Extension
        {
            get => _file.Extension;
        }

        public string Group
        {
            get => _file.TagGroup;
        }

        public IHIRTFile File
        {
            get => _file;
        }
        public bool GenericView { get => _genericView; set => _genericView = value; }

        #endregion

        #region Constructor

        public FileModel(IHIRTFile file)
        {
            _file = file;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _file.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FileModel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            //GC.SuppressFinalize(this);
        }

        #endregion

    }

}
