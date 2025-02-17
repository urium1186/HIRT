using HaloInfiniteResearchTools.Common;
using LibHIRT.Files.FileTypes;
using System;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(HsSymbolFile))]
    public class HsSymbolViewModel : SSpaceFileViewModel<HsSymbolFile>
    {
        public HsSymbolViewModel(IServiceProvider serviceProvider, HsSymbolFile file) : base(serviceProvider, file)
        {
        }

        protected override Task OnInitializing()
        {
            return base.OnInitializing();
        }
    }
}
