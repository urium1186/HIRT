using HaloInfiniteResearchTools.ViewModels;
using System.Windows;

namespace HaloInfiniteResearchTools.Views
{
    /// <summary>
    /// Interaction logic for ToolsView.xaml
    /// </summary>
    public partial class ToolsView :  View<ToolsViewModel>
    {
        public ToolsView()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (DataContext is ToolsViewModel)
            {
                if ((DataContext as ToolsViewModel).CheckInUse())
                {
                    MessageBox.Show("Is in Use");
                }
                else {
                    MessageBox.Show("Not in Use");
                }
            }
        }

        private void AddToDB_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ToolsViewModel)
            {
                if ((DataContext as ToolsViewModel).CheckInUse())
                {
                    if ((DataContext as ToolsViewModel).AddUniqueStrValue())
                        MessageBox.Show("Saved to DB");
                    else {
                        MessageBox.Show("Not need to save.");
                    }
                }
                else
                {
                    MessageBoxOptions options = default;
                    var result = MessageBox.Show(App.Current.MainWindow, "Not in Use, do you want to add???", "Save Mmh3",MessageBoxButton.OKCancel,MessageBoxImage.Warning,MessageBoxResult.Cancel);
                    if (result == MessageBoxResult.OK) {
                        (DataContext as ToolsViewModel).AddUniqueStrValue();
                        MessageBox.Show("Saved to DB");
                    }
                }
                
            }
        }

        

    }
}
