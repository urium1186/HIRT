using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files.FileTypes;
using System;

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
