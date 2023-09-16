using HaloInfiniteResearchTools.ViewModels;
using HaloInfiniteResearchTools.Views;
using System;

namespace HaloInfiniteResearchTools.Services
{

    public interface IViewService
    {

        IView GetView(IViewModel viewModel);

        IView GetViewWithDefaultViewModel(Type viewType);

    }

}
