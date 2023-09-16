using HaloInfiniteResearchTools.Models;
using LibHIRT.Files;
using LibHIRT.TagReader.Common;
//using Saber3D.Files;

namespace HaloInfiniteResearchTools.Services
{

    public interface ITabService
    {

        #region Properties

        TabContextModel TabContext { get; }

        #endregion

        #region Public Methods

        bool CreateTabForFile(ISSpaceFile file, out ITab tab, bool forceGeneric = false);
        bool CreateTabForFile(TagStructMem file, out ITab tab, bool forceGeneric = false);

        #endregion

    }

}
