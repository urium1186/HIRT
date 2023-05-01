using LibHIRT.Domain;
using LibHIRT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public GenericFile(string name, HIRTStream baseStream,
          long dataStartOffset, long dataEndOffset,
          ISSpaceFile parent = null)
          : base(name, baseStream, dataStartOffset, dataEndOffset, parent)
        {
        }

        
        

        #endregion
    }
}
