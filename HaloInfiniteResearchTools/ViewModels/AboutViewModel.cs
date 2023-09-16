using HaloInfiniteResearchTools.ViewModels.Abstract;
using System;
using System.Reflection;

namespace HaloInfiniteResearchTools.ViewModels
{

    public class AboutViewModel : ViewModel, IDisposeWithView
    {

        #region Properties

        public string VersionString
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return $"v{version.Major}.{version.Minor}.{version.Revision}";
            }
        }

        #endregion

        #region Constructor

        public AboutViewModel(IServiceProvider serviceProvider)
          : base(serviceProvider)
        {
        }

        #endregion

    }

}
