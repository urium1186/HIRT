using HaloInfiniteResearchTools.ViewModels.Online;
using System.Windows;
using System.Windows.Controls;

namespace HaloInfiniteResearchTools.Views
{
    /// <summary>
    /// Interaction logic for CustomOnlineView.xaml
    /// </summary>
    public partial class CustomOnlineView : View<CustomOnlineViewModel>
    {
        public CustomOnlineView()
        {
            InitializeComponent();
        }

        private void dataGridThemes_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void dataGridThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dc = DataContext as CustomOnlineViewModel;
            if (dc != null)
            {
                if (e.AddedItems.Count > 0)
                {
                    var data = (e.AddedItems[0] as DataGridRowData);
                    if (data != null)
                    {
                        dc.OnGridChangeSelectionCommand.Execute(data);
                    }
                    else
                    {
                        dc.OnGridChangeSelectionCommand.Execute(null);
                    }

                }
                else
                {
                    dc.OnGridChangeSelectionCommand.Execute(null);
                }

            }
        }

        private void dataGridThemes_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {

        }
    }
}
