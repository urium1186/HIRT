using LibHIRT.Files;

namespace HaloInfiniteResearchTools.Models
{
    public interface IFileModel
    {
    }
    public class FileModel : IFileModel
    {

        #region Data Members

        private readonly ISSpaceFile _file;

        #endregion

        #region Properties

        private bool _genericView = false;
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
            get => _file.FileTypeDisplay;
        }

        public ISSpaceFile File
        {
            get => _file;
        }
        public bool GenericView { get => _genericView; set => _genericView = value; }

        #endregion

        #region Constructor

        public FileModel(ISSpaceFile file)
        {
            _file = file;
        }

        #endregion

    }

}
