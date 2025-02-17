using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using System;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(WeaponDefinitionFile))]
    public class WeaponDefinitionViewModel : ViewModel, IDisposeWithView
    {
        ModelFile? _file;
        public WeaponDefinitionViewModel(IServiceProvider serviceProvider, SSpaceFile file) : base(serviceProvider)
        {
            _file = ((WeaponDefinitionFile?)file)?.GetModel();
        }

        public ModelFile? File { get => _file; set => _file = value; }

        protected override async Task OnInitializing()
        {
            await base.OnInitializing();
        }
    }
}
