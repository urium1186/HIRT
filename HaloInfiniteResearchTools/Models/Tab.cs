using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Views;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HaloInfiniteResearchTools.Models
{

    public class Tab : ITab
    {

        #region Events

        public event EventHandler CloseRequested;
        public event EventHandler CloseAllTabRequested;
        public event EventHandler CloseOthersTabRequested;
        public event EventHandler CloseLeftTabRequested;
        public event EventHandler CloseRightTabRequested;

        #endregion

        #region Data Members

        private bool _isDisposed;

        #endregion

        #region Properties

        public string Name { get; }
        public IView View { get; }

        public ICommand CloseCommand { get; }
        public ICommand CloseAllTabCommand { get; }
        public ICommand CloseOthersTabCommand { get; }
        public ICommand CloseLeftTabCommand { get; }
        public ICommand CloseRightTabCommand { get; }

        #endregion

        #region Constructor

        public Tab(string name, IView view)
        {
            Name = name;
            View = view;
            CloseCommand = new Command(Close);
            CloseAllTabCommand = new Command(CloseAllTab);
            CloseOthersTabCommand = new Command(CloseOthersTab);
            CloseLeftTabCommand = new Command(CloseLeftTab);
            CloseRightTabCommand = new Command(CloseRightTab);
        }

        #endregion

        #region Public Methods

        public void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        public void CloseAllTab()
        {
            CloseAllTabRequested?.Invoke(this, EventArgs.Empty);
        }
         public void CloseOthersTab()
        {
            CloseOthersTabRequested?.Invoke(this, EventArgs.Empty);
        }
         public void CloseLeftTab()
        {
            CloseLeftTabRequested?.Invoke(this, EventArgs.Empty);
        }
         public void CloseRightTab()
        {
            CloseRightTabRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        protected virtual Task OnInitializing()
          => Task.CompletedTask;

        #endregion

        #region IDisposable Methods

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
                OnDisposing();

            _isDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            View?.Dispose();
        }

        #endregion

    }

}
