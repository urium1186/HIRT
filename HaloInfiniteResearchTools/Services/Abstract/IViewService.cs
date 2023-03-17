using System;
using HaloInfiniteResearchTools.ViewModels;
using HaloInfiniteResearchTools.Views;

namespace HaloInfiniteResearchTools.Services
{

  public interface IViewService
  {

    IView GetView( IViewModel viewModel );

    IView GetViewWithDefaultViewModel( Type viewType );

  }

}
