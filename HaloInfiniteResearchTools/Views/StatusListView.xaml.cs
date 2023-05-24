using HaloInfiniteResearchTools.ViewModels;

namespace HaloInfiniteResearchTools.Views
{
    /// <summary>
    /// Interaction logic for StatusListView.xaml
    /// </summary>
    public partial class StatusListView : View<StatusListViewModel>
    {

        public override string ViewName
        {
            get => "Process Results";
        }

        public StatusListView()
        {
            InitializeComponent();
        }

    }
}
