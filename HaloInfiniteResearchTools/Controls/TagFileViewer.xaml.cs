using HaloInfiniteResearchTools.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HaloInfiniteResearchTools.Controls
{
    /// <summary>
    /// Interaction logic for TagFileViewer.xaml
    /// </summary>
    public partial class TagFileViewer : UserControl
    {
        public TagFileViewer()
        {
            InitializeComponent();
        }

        private void TagRefIdGenButton_Click(object sender, RoutedEventArgs e)
        {
            GenericViewModel temp = DataContext as GenericViewModel;

            if (temp != null)
                temp.OpenGenFileTabRefIntCommand?.Execute((int)(sender as Button).DataContext);
        }
    }
}
