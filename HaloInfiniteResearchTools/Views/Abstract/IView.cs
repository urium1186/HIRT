using System;
using HaloInfiniteResearchTools.ViewModels;

namespace HaloInfiniteResearchTools.Views
{

  public interface IView : IDisposable
  {

    #region Properties

    string ViewName { get; }

    object DataContext { get; set; }

    #endregion

  }

  public interface IView<TViewModel> : IView
    where TViewModel : IViewModel
  {

    #region Properties

    TViewModel ViewModel { get; }

    #endregion

  }

}
