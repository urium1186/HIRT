using Assimp;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.ViewModels;
using System.Linq;
using System.Windows;

namespace HaloInfiniteResearchTools.Controls
{
    public class ParNodes { 
        public Scene Attach { get; set; }
        public Node Marker { get; set; }
    }

    public class ItemHelper : DependencyObject
    {
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.RegisterAttached("IsChecked", typeof(bool?), typeof(ItemHelper), new PropertyMetadata(false, new PropertyChangedCallback(OnIsCheckedPropertyChanged)));
        
        private static void OnIsCheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IParentModel<ICheckedModel> && ((bool?)e.NewValue).HasValue)
                foreach (ICheckedModel p in (d as TreeViewItemModel).Children)
                    ItemHelper.SetIsChecked((DependencyObject)p, (bool?)e.NewValue);

            if (d is TreeViewItemChModel)
            {/**/
                var ch = d as TreeViewItemChModel;
                if (ch != null) {
                    if (ch.Value != null) {
                        var node = ch.Value as ModelNodeModel;
                        
                        if (node!= null)
                        {
                            node.Node.Tag = GetParentAttachmentTransform(ch); 
                            
                            node.IsVisible = (bool)e.NewValue;
                        }
                    }
                }
                int _checked = ((d as TreeViewItemChModel).GetValue(ItemHelper.ParentProperty) as TreeViewItemModel).Children.Where(x => {
                    return ItemHelper.GetIsChecked((DependencyObject)x) == true;
                }).Count();
                int _unchecked = ((d as TreeViewItemChModel).GetValue(ItemHelper.ParentProperty) as TreeViewItemModel).Children.Where(x => ItemHelper.GetIsChecked((DependencyObject)x) == false).Count();
                if (_unchecked > 0 && _checked > 0)
                {
                    ItemHelper.SetIsChecked((d as TreeViewItemChModel).GetValue(ItemHelper.ParentProperty) as DependencyObject, null);
                    return;
                }
                if (_checked > 0)
                {
                    ItemHelper.SetIsChecked((d as TreeViewItemChModel).GetValue(ItemHelper.ParentProperty) as DependencyObject, true);
                    return;
                }
                ItemHelper.SetIsChecked((d as TreeViewItemChModel).GetValue(ItemHelper.ParentProperty) as DependencyObject, false);
            }
        }

        public static void SetIsChecked(DependencyObject element, bool? IsChecked)
        {
            element.SetValue(ItemHelper.IsCheckedProperty, IsChecked);
        }
        public static bool? GetIsChecked(DependencyObject element)
        {
            return (bool?)element.GetValue(ItemHelper.IsCheckedProperty);
        }

        public static readonly DependencyProperty ParentProperty = DependencyProperty.RegisterAttached("Parent", typeof(object), typeof(ItemHelper));
        public static void SetParent(DependencyObject element, object Parent)
        {
            element.SetValue(ItemHelper.ParentProperty, Parent);
        }
        public static object GetParent(DependencyObject element)
        {
            return (object)element.GetValue(ItemHelper.ParentProperty);
        }

        public static ParNodes GetParentAttachmentTransform(TreeViewItemChModel element)
        {
            bool noStop = true;
            TreeViewItemModel parent = (element.GetValue(ItemHelper.ParentProperty) as TreeViewItemModel);
            while (noStop)
            {
                
                if (parent == null || parent.Tag is ParNodes) { 
                    noStop= false;
                }
                if (noStop)
                    parent = (parent.GetValue(ItemHelper.ParentProperty) as TreeViewItemModel);
            }
            if (parent != null)
                return (ParNodes)parent.Tag;
            return null;
        }
    }
}
