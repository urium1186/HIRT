using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using System;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(VehicleDefinitionFile))]
    public class VehicleDefinitionViewModel : ViewModel, IDisposeWithView
    {
        ModelFile? _file;
        public VehicleDefinitionViewModel(IServiceProvider serviceProvider, SSpaceFile file) : base(serviceProvider)
        {
            _file = ((VehicleDefinitionFile?)file)?.GetModel();
        }

        public ModelFile? File { get => _file; set => _file = value; }

        protected override async Task OnInitializing()
        {
            await base.OnInitializing();
        }
    }
}
