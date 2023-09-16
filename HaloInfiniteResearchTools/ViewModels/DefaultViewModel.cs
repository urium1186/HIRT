using HaloInfiniteResearchTools.ViewModels.Abstract;
using System;

namespace HaloInfiniteResearchTools.ViewModels
{
    public class DefaultViewModel : ViewModel, IDisposeWithView
    {

        #region Constructor

        public DefaultViewModel(IServiceProvider serviceProvider)
          : base(serviceProvider)
        {
        }

        #endregion

    }

}
