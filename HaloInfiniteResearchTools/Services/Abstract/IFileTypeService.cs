using System;
using System.Collections.Generic;

namespace HaloInfiniteResearchTools.Services
{

    public interface IFileTypeService
    {

        #region Data Members

        IReadOnlySet<string> ExtensionsWithEditorSupport { get; }

        #endregion

        #region Public Methods

        Type GetViewModelType(Type fileType);

        #endregion

    }

}
