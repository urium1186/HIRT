using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HaloInfiniteResearchTools.UI.Modals;

namespace HaloInfiniteResearchTools.ViewModels
{

  public interface IViewModel : IDisposable
  {

    ObservableCollection<IModal> Modals { get; }

    Task Initialize();

  }

}
