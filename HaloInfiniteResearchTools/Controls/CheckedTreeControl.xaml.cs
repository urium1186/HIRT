using HaloInfiniteResearchTools.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace HaloInfiniteResearchTools.Controls
{
    /// <summary>
    /// Interaction logic for CheckedTreeControl.xaml
    /// </summary>
    /// 

    public partial class CheckedTreeControl : UserControl
    {

        public static readonly DependencyProperty TreeItemsProperty = DependencyProperty.Register("TreeItems", typeof(ObservableCollection<TreeViewItemModel>), typeof(CheckedTreeControl), new
          PropertyMetadata(null, new PropertyChangedCallback(OnTreeItemsChanged)));

        public Stream TreeItems
        {
            get { return (Stream)GetValue(TreeItemsProperty); }
            set { SetValue(TreeItemsProperty, value); }
        }
        ObservableCollection<TreeViewItemModel> _treeItems;
        private static void OnTreeItemsChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            CheckedTreeControl userCtrl = d as CheckedTreeControl;
            userCtrl.OnTreeItemsChanged(e);
        }

        private void OnTreeItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                treeView.ItemsSource = (ObservableCollection<TreeViewItemModel>)e.NewValue;

            }
        }

        public CheckedTreeControl()
        {
            InitializeComponent();
            ObservableCollection<TreeViewItemModel> treeItems = new ObservableCollection<TreeViewItemModel>();
            var temp = new TreeViewItemModel();
            temp.Header = "prueba";

            var temp2 = new TreeViewItemChModel();
            temp2.Header = "prueba2";
            var temp3 = new TreeViewItemModel();
            temp3.Header = "prueba";

            var temp4 = new TreeViewItemChModel();
            temp4.Header = "prueba4";
            temp4.SetValue(ItemHelper.ParentProperty, temp3);
            temp3.Children.Add(temp4);
            temp3.SetValue(ItemHelper.ParentProperty, temp);
            temp2.SetValue(ItemHelper.ParentProperty, temp);
            temp.Children.Add(temp2);
            temp.Children.Add(temp3);
            treeItems.Add(temp);
            // Crea los elementos y agrega a treeItems
            treeView.ItemsSource = treeItems;
        }
    }
}
