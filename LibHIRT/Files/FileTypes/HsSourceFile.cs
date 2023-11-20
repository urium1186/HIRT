using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Files.FileTypes
{
    [FileSignature("16D12561C1431245B57D89A657DC2C80")]
    [FileExtension(".lua0_sourceresou")]
    public class HsSourceFile : SSpaceFile
    {
        public HsSourceFile(string name, ISSpaceFile parent = null) : base(name, parent)
        {
            GroupRefHash = ("hsc*", "16D12561C1431245B57D89A657DC2C80");
        }

        public override string FileTypeDisplay => "HsSource (.lua0_sourceresou)";
    }
}
