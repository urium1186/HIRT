using HaloInfiniteResearchTools.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HaloInfiniteResearchTools.Views
{
    /// <summary>
    /// Interaction logic for PsoDictionaryView.xaml
    /// </summary>
    public partial class PsoDictionaryView : View<PsoDictionaryViewModel>
    {
        public PsoDictionaryView()
        {
            InitializeComponent();
        }

        private void TagRefGenButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = (DataContext as PsoDictionaryViewModel);
            vm.OpenGenFileViewCommand.Execute((int)(sender as Button).DataContext);
        }
    }
}
