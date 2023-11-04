using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Views;
using LibHIRT.Files;
using LibHIRT.Files.Base;
using System.Collections.Generic;
using System.ComponentModel;
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

        public static readonly DependencyProperty FileTreeExportJsonCommandProperty = DependencyProperty.Register(
          nameof(FileTreeExportJsonCommand),
          typeof(ICommand),
          typeof(FileTreeControl),
          new PropertyMetadata());


        public static readonly DependencyProperty FileTreeExportRecursiveJsonCommandProperty = DependencyProperty.Register(
          nameof(FileTreeExportRecursiveJsonCommand),
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
        public ICommand FileTreeExportJsonCommand
        {
            get => (ICommand)GetValue(FileTreeExportJsonCommandProperty);
            set => SetValue(FileTreeExportJsonCommandProperty, value);
        }
        public ICommand FileTreeExportRecursiveJsonCommand
        {
            get => (ICommand)GetValue(FileTreeExportRecursiveJsonCommandProperty);
            set => SetValue(FileTreeExportRecursiveJsonCommandProperty, value);
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

            var file = item.Header as IHIRTFile;
            if (!(file is null))
            {
                e.Handled = true;
                FileDoubleClickCommand?.Execute((file, false));
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
                //FileTree.UpdateLayout(); {Name = "CollectionViewGroup" FullName = "System.Windows.Data.CollectionViewGroup"}
            }
        }

        private void MenuItem_ExportToJsonClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem; if (item == null)
            {
                return;
            }
            var file = item.DataContext as IHIRTFile;
            List<IHIRTFile> files = new List<IHIRTFile>();
            if (!(file is null))
            {
                e.Handled = true;

                files.Add(file);
                
            }
            else if (item.DataContext is CollectionViewGroup)
            {
                CollectionViewGroup group= (CollectionViewGroup)item.DataContext;

                foreach (var file_in in group.Items)
                {
                    if (file_in is IHIRTFile) {
                        files.Add((IHIRTFile)file_in);
                    }
                }
            }
            FileTreeExportJsonCommand?.Execute(files);
        }

        private void MenuItem_OpenGenericViewClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem; if (item == null)
            {
                return;
            }
            var file = item.DataContext as IHIRTFile;
            if (!(file is null))
            {
                e.Handled = true;
                FileDoubleClickCommand?.Execute((file, true));
                
            }
        }

        private void MenuItem_ExportRecursiveToJsonClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem; if (item == null)
            {
                return;
            }
            var fileModel = item.DataContext as IHIRTFile;
            if (!(fileModel is null))
            {
                e.Handled = true;


                FileTreeExportRecursiveJsonCommand?.Execute(fileModel);
            }
        }
    }

}
