using LibHIRT.Common;

namespace LibHIRT.Files.Base
{
    public abstract class SSpaceContainerFile : SSpaceFile
    {

        #region Constructor

        protected SSpaceContainerFile(string name, HIRTStream baseStream,
          long dataStartOffset, long dataEndOffset,
          ISSpaceFile parent = null)
          : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
        }

        #endregion

        #region Overrides

        protected override void OnInitialize()
        {
            BaseStream.Position = 0;

            ReadHeader();
            ReadChildren();
        }

        #endregion

        #region Private Methods

        protected virtual ISSpaceFile CreateChildFile(string name, long offset, long size, string signature)
        {
            var dataStartOffset = CalculateTrueChildOffset(offset);
            var dataEndOffset = dataStartOffset + size;

            return SSpaceFileFactory.CreateFile(name, BaseStream, dataStartOffset, dataEndOffset, signature,this);
        }

        protected long CalculateTrueChildOffset(long offset)
          => DataStartOffset + offset;

        protected virtual void ReadHeader()
        {
            // TODO
            // Skipping this for now. We might want to read it though.
            // BaseStream.Position = DataStartOffset + 0x45;
        }

        protected virtual void ReadChildren()
        {
            

        }

        protected override string SanitizeName(string fileName)
        {
            fileName = base.SanitizeName(fileName);

            /* A lot of times, files will be encapsulated in a Pck file, but will still
             * use the extension of the file they contain. We don't want these Pck containers
             * to use their child file's extension. The files within these containers have
             * offsets to data that do not account for the Pck header to be present.
             * 
             * In the UI, we'll want to hide Pck files from the FileTree to avoid confusion.
             */

            var ext = Path.GetExtension(fileName);
            if (ext != ".module")
                fileName = Path.ChangeExtension(fileName, ".module");

            return fileName;
        }

        #endregion

    }

}
