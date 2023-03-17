using HaloInfiniteResearchTools.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        bool IsSelected { get; set; }
        bool IsChecked { get; set; }
        //T Value { get; set; }
    }

    public class TreeViewItemModel : DependencyObject, IParentModel<ICheckedModel>
    {
        private Collection<ICheckedModel> _childrens = new Collection<ICheckedModel>();

        public Collection<ICheckedModel> Children => _childrens;

        public string Header { get; set; }
        public bool IsSelected { get; set; }
        public bool IsChecked { get; set; }

        public TreeViewItemModel() { }
    }

    public class TreeViewItemChModel : DependencyObject, ICheckedModel
    {
        private bool _isChecked;

        public string Header { get; set; }
        public bool IsSelected { get; set; }
        public bool IsChecked { get; set; }
        public object Value { get; set; }

        public TreeViewItemChModel() { }
    }

}
