using HaloInfiniteResearchTools.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
