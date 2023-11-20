using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Files.FileTypes
{
    [FileTagGroup("jssc")]
    [FileSignature("756E3837385A6921")]
    [FileExtension(".jssc")]
    public class JsonSourceFileTagDefinitionFile : SSpaceFile
    {
        public JsonSourceFileTagDefinitionFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
        }

        public override string FileTypeDisplay => "JsonSourceFileTagDefinition (.jssc)";
    }
}
