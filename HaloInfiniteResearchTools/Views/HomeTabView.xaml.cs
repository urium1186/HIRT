using HaloInfiniteResearchTools.ViewModels;
using System.Windows;

namespace HaloInfiniteResearchTools.Views
{
    /// <summary>
    /// Interaction logic for HomeTabView.xaml
    /// </summary>
    public partial class HomeTabView : View<HomeTabViewModel>
    {
        public HomeTabView()
        {
            InitializeComponent();
        }

        public HomeTabView(bool contentLoaded)
        {
            _contentLoaded = contentLoaded;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
