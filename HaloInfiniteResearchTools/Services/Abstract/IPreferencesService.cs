using HaloInfiniteResearchTools.Models;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Services
{

    public interface IPreferencesService
    {

        #region Properties

        PreferencesModel Preferences { get; }

        #endregion

        #region Public Methods

        Task Initialize();

        Task<PreferencesModel> LoadPreferences();
        Task SavePreferences();

        #endregion

    }

}
