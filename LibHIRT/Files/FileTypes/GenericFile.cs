namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("_*.*")]
    [FileSignature("FFFFFFFFFFFFFFFF")]
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
