using HaloInfiniteResearchTools.Models;
using LibHIRT.Files.Base;
//using Saber3D.Files;

namespace HaloInfiniteResearchTools.Services
{

    public interface ITabService
    {

        #region Properties

        TabContextModel TabContext { get; }

        #endregion

        #region Public Methods

        bool CreateTabForFile(IHIRTFile file, out ITab tab, bool forceGeneric = false);
        bool createHomeTab();
        void CloseAllTab();

        #endregion

    }

}
