using HaloInfiniteResearchTools.Controls;
using System.Collections.ObjectModel;
using System.Windows;

namespace HaloInfiniteResearchTools.Models
{

    public interface IParentModel<T> : ICheckedModel where T : ICheckedModel
    {
        Collection<T> Children { get; }
    }

    public interface ICheckedModel
    {
        string Header { get; set; }
        object Tag { get; set; }
        bool IsSelected { get; set; }
        bool IsChecked { get; set; }

        ICheckedModel copy();
        //T Value { get; set; }
    }

    public abstract class CheckedModel : DependencyObject, ICheckedModel
    {
        public string Header { get; set; }
        public object Tag { get; set; }
        public bool IsSelected { get; set; }
        public bool IsChecked { get; set; }

        public abstract ICheckedModel copy();
        //public T Value { get; set; }
    }

    public class TreeViewItemModel : CheckedModel, IParentModel<ICheckedModel>
    {
        private Collection<ICheckedModel> _childrens = new Collection<ICheckedModel>();

        public Collection<ICheckedModel> Children => _childrens;

        public string Header { get; set; }
        public bool IsSelected { get; set; }
        public bool IsChecked { get; set; }

        public object Tag { get; set; }

        public TreeViewItemModel() { }

        public override ICheckedModel copy()
        {
            TreeViewItemModel result = new TreeViewItemModel()
            {
                IsChecked = IsChecked,
                IsSelected = IsSelected,
                Header = Header,
                Tag = Tag,
            };
            foreach (CheckedModel child in Children)
            {
                var c_c = child.copy() as CheckedModel;
                c_c.SetValue(ItemHelper.ParentProperty, result);
                result.Children.Add(c_c);
            }
            return result;
        }
    }

    public class TreeViewItemChModel : CheckedModel, ICheckedModel
    {
        private bool _isChecked;

        public string Header { get; set; }
        public bool IsSelected { get; set; }
        public bool IsChecked { get; set; }
        public object Value { get; set; }

        public object Tag { get; set; }

        public TreeViewItemChModel() { }

        public override ICheckedModel copy()
        {
            var r = new TreeViewItemChModel
            {
                Header = Header,
                IsSelected = IsSelected,
                Value = Value,
                Tag = Tag,
                IsChecked = IsChecked,
            };

            return r;
        }
    }

}
