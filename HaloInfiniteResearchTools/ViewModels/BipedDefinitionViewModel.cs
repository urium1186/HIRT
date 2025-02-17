using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.Files.FileTypes;
using System;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(BipedDefinitionFile))]
    public class BipedDefinitionViewModel : ViewModel, IDisposeWithView
    {
        ModelFile? _file;

        public BipedDefinitionViewModel(IServiceProvider serviceProvider, SSpaceFile file) : base(serviceProvider)
        {
            _file = ((HasModel?)file)?.GetModel();
        }
        public ModelFile? File { get => _file; set => _file = value; }

        protected override async Task OnInitializing()
        {
            await base.OnInitializing();
        }
    }
}
