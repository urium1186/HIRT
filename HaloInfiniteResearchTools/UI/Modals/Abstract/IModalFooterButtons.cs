using System.Collections.Generic;
using System.Windows.Controls;

namespace HaloInfiniteResearchTools.UI.Modals
{

    public interface IModalFooterButtons
    {

        IEnumerable<Button> GetFooterButtons();

    }

}
