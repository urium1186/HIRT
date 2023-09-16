using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files.FileTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(ScenarioStructureBspFile))]
    public class ScenarioStructureBspViewModel : ViewModel, IDisposeWithView
    {
        public ScenarioStructureBspViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
