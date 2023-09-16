using HaloInfiniteResearchTools.ViewModels;

namespace HaloInfiniteResearchTools.Views
{
    /// <summary>
    /// Interaction logic for LevelView.xaml
    /// </summary>
    public partial class LevelView : View<LevelViewModel>
    {
        public LevelView()
        {
            InitializeComponent();
        }

        protected override void OnDisposing()
        {
            ModelViewer?.Dispose();
            base.OnDisposing();
        }
    }


}
