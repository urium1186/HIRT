using HaloInfiniteResearchTools.Models;
using LibHIRT.Files;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace HaloInfiniteResearchTools.Controls
{

    public partial class FileTreeControl : UserControl
    {

        #region Data Members

        public static readonly DependencyProperty FileDoubleClickCommandProperty = DependencyProperty.Register(
          nameof(FileDoubleClickCommand),
          typeof(ICommand),
          typeof(FileTreeControl),
          new PropertyMetadata());

        #endregion

        #region Properties

        public ICommand FileDoubleClickCommand
        {
            get => (ICommand)GetValue(FileDoubleClickCommandProperty);
            set => SetValue(FileDoubleClickCommandProperty, value);
        }

        #endregion

        #region Constructor

        public FileTreeControl()
        {
            InitializeComponent();
            //FileMTree.IsLiveSorting = true;
        }

        #endregion

        #region Private Methods

        private TreeViewItem TryGetClickedItem(MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as DependencyObject;
            while (hit != null && !(hit is TreeViewItem))
                hit = VisualTreeHelper.GetParent(hit);

            return hit as TreeViewItem;
        }

        #endregion

        #region Event Handlers

        private void OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = TryGetClickedItem(e);
            if (item is null)
                return;

            var fileModel = item.Header as FileDirModel;
            if (!(fileModel is null))
            {
                e.Handled = true;
                var temp_model = new FileModel(fileModel.File);
                if (e.LeftButton == MouseButtonState.Released)
                {
                    temp_model.GenericView = true;
                }
                FileDoubleClickCommand?.Execute(temp_model);
            }
            else {
                var treeModel = item.Header as TreeHierarchicalModel;
                
                if (treeModel != null) {
                    FileDoubleClickCommand?.Execute(treeModel);
                }
            }
            

            
        }

        #endregion

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SearchBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void SearchBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //var temp = DataContext;
                //DataContext = null;
                BindingOperations.GetBindingExpressionBase((TreeView)FileTree, TreeView.ItemsSourceProperty).UpdateTarget();
                //DataContext = temp;
                //FileTree.Items.Refresh();
                //FileTree.UpdateLayout();
            }
        }
    }

}
