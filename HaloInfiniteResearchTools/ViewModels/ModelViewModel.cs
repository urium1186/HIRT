using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using System;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(ModelFile))]
    public class ModelViewModel : ViewModel, IDisposeWithView
    {
        ModelFile? _file;
        public ModelViewModel(IServiceProvider serviceProvider, SSpaceFile file) : base(serviceProvider)
        {
            _file = (ModelFile?)file;

        }

        public ModelFile? File { get => _file; set => _file = value; }

        protected override async Task OnInitializing()
        {
            await base.OnInitializing();

        }
    }
}
