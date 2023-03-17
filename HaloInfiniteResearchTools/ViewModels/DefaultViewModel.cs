using HaloInfiniteResearchTools.ViewModels.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
