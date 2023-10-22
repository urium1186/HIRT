using HaloInfiniteResearchTools.Views;
using System;
using System.Windows.Input;

namespace HaloInfiniteResearchTools.Models
{
    public interface ITab : IDisposable
    {

        #region Events

        event EventHandler CloseRequested;
        event EventHandler CloseAllTabRequested;
        event EventHandler CloseOthersTabRequested;
        event EventHandler CloseLeftTabRequested;
        event EventHandler CloseRightTabRequested;

        #endregion

        #region Properties

        string Name { get; }
        IView View { get; }

        ICommand CloseCommand { get; }
        ICommand CloseAllTabCommand { get; }
        ICommand CloseOthersTabCommand { get; }
        ICommand CloseLeftTabCommand { get; }
        ICommand CloseRightTabCommand { get; }

        #endregion

        #region Public Methods

        void Close();

        #endregion

    }
}
