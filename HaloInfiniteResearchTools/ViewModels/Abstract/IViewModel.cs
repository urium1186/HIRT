using HaloInfiniteResearchTools.UI.Modals;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{

    public interface IViewModel : IDisposable
    {

        ObservableCollection<IModal> Modals { get; }

        Task Initialize();

    }

}
