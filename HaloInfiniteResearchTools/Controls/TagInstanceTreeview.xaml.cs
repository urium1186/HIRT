using HaloInfiniteResearchTools.Models;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HaloInfiniteResearchTools.Controls
{
    /// <summary>
    /// Interaction logic for TagInstanceTreeview.xaml
    /// </summary>
    public partial class TagInstanceTreeview : UserControl
    {
        public static readonly DependencyProperty FileDoubleClickCommandProperty = DependencyProperty.Register(
         nameof(FileDoubleClickCommand),
         typeof(ICommand),
         typeof(TagInstanceTreeview),
         new PropertyMetadata());





        public ICommand FileDoubleClickCommand
        {
            get => (ICommand)GetValue(FileDoubleClickCommandProperty);
            set => SetValue(FileDoubleClickCommandProperty, value);
        }

        public TagInstanceTreeview()
        {
            InitializeComponent();
        }

        private void TagRefButton_Click(object sender, RoutedEventArgs e)
        {
            TagRef tagRef = (TagRef)((sender as Button)?.DataContext);
            if (tagRef.Ref_id_int != -1)
                FileDoubleClickCommand?.Execute(tagRef);
        }

    }
}
