using HaloInfiniteResearchTools.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HaloInfiniteResearchTools.Views
{
    /// <summary>
    /// Interaction logic for ModuleIndexFileView.xaml
    /// </summary>
    public partial class ModuleIndexFileView : View<ModuleIndexFileViewModel>
    {
        public ModuleIndexFileView()
        {
            InitializeComponent();
        }

        private void TagRefGenButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (DataContext as ModuleIndexFileViewModel);
            vm.OpenGenFileViewCommand.Execute((int)(sender as Button).DataContext);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
