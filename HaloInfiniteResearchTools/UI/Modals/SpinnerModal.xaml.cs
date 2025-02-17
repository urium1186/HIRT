using HaloInfiniteResearchTools.ViewModels;

namespace HaloInfiniteResearchTools.UI.Modals
{
    /// <summary>
    /// Interaction logic for SpinnerModal.xaml
    /// </summary>
    public partial class SpinnerModal : BoundModal<ProgressViewModel>
    {
        public SpinnerModal(ProgressViewModel viewModel)
          : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
