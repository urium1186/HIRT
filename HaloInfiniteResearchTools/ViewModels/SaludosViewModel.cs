using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using System;
using System.Windows.Input;

namespace HaloInfiniteResearchTools.ViewModels
{
    public class SaludosViewModel : ViewModel, IDisposeWithView
    {
        public event EventHandler<MainSubView> onChangeView;
        public ICommand OpenViewOnlineCommand { get; }
        public ICommand OpenViewRippingCommand { get; }
        public SaludosViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            OpenViewRippingCommand = new Command(OpenViewRipping);
            OpenViewOnlineCommand = new Command(OpenViewOnline);
        }

        private async void OpenViewOnline()
        {
            if (onChangeView != null)
                onChangeView.Invoke(this, MainSubView.Online);
        }


        private void OpenViewRipping()
        {
            if (onChangeView != null)
                onChangeView.Invoke(this, MainSubView.Riping);
        }
    }
}
