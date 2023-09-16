using HaloInfiniteResearchTools.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HaloInfiniteResearchTools.Views
{
    /// <summary>
    /// Interaction logic for PreferencesView.xaml
    /// </summary>
    public partial class PreferencesView : View<PreferencesViewModel>
    {

        public PreferencesView()
        {
            InitializeComponent();
        }

        protected override void OnDisposing()
        {
            ViewModel?.Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PreferencesViewModel)
            {
                (DataContext as PreferencesViewModel).removePath((Models.TagReaderPath)(sender as Button).DataContext);
            }
        }
    }
}
