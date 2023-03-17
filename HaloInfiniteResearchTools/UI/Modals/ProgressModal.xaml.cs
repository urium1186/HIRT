using HaloInfiniteResearchTools.ViewModels;


namespace HaloInfiniteResearchTools.UI.Modals
{
    /// <summary>
    /// Interaction logic for ProgressModal.xaml
    /// </summary>
    public partial class ProgressModal : BoundModal<ProgressViewModel>
    {
        #region Constructor

        public ProgressModal(ProgressViewModel viewModel)
          : base(viewModel)
        {
            InitializeComponent();
        }

        #endregion

    }
}
