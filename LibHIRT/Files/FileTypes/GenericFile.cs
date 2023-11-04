namespace LibHIRT.Files.FileTypes
{
    [FileSignature("_*.*")]
    [FileExtension(".*")]
    public class GenericFile : SSpaceFile
    {

        #region Properties

        public override string FileTypeDisplay => "Generic (*.*)";

        #endregion

        #region Constructor

        public GenericFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }




        #endregion
    }
}
