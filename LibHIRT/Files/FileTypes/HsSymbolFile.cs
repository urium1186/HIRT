using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Files.FileTypes
{
    [FileSignature("A40842F0184C32C15A4F3EB474014091")]
    [FileExtension(".lua1_symbolresou")]
    public class HsSymbolFile : SSpaceFile
    {
        public HsSymbolFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
            GroupRefHash = ("hsc*", "A40842F0184C32C15A4F3EB474014091");
        }

        public override string FileTypeDisplay => "HsSymbol (.lua1_symbolresou)";
    }
}
