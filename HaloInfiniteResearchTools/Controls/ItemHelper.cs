using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HaloInfiniteResearchTools.Controls
{
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
    }
}
