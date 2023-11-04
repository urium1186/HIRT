using HaloInfiniteResearchTools.Models;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.TagReader.Common;
using LibHIRT.TagReader.RuntimeViewer;
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
        void CloseAllTab();

        #endregion

    }

}
