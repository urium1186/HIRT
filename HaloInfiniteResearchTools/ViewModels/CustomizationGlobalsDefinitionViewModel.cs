using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(CustomizationGlobalsDefinitionFile))]
    public class CustomizationGlobalsDefinitionViewModel : ViewModel, IDisposeWithView
    {
        CustomizationGlobalsDefinitionFile? _file;
        public CustomizationGlobalsDefinitionViewModel(IServiceProvider serviceProvider, SSpaceFile file) : base(serviceProvider)
        {
            _file = (CustomizationGlobalsDefinitionFile)file;
        }

        public CustomizationGlobalsDefinitionFile? File { get => _file; set => _file = value; }
    }
}
