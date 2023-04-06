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
    /// Interaction logic for GenericView.xaml
    /// </summary>
    public partial class GenericView : View<GenericViewModel>
    {
        public GenericView()
        {
            InitializeComponent();
            
        }



        protected override void OnDisposing()
        {
            model3dCtrl?.Dispose();
            base.OnDisposing();
        }

        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            var temp = DataContext as GenericViewModel;
            if (temp != null)
                webBro.Navigate(ViewModel.XmlPath);
        }

        private void XmlViewer_Initialized(object sender, EventArgs e)
        {
            
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0) {
                var tb = (e.AddedItems[0] as TabItem);
                if (tb != null && tb.Name == "XmlViewer")
                {
                    var temp = DataContext as GenericViewModel;
                    if (temp != null && webBro.Source == null && !string.IsNullOrEmpty(temp.XmlPath))
                        webBro.Navigate(temp.XmlPath);
                }
            }
            
            
        }
    }
}
