using HaloInfiniteResearchTools.Models;
using LibHIRT.TagReader;
using SharpDX.DirectComposition;
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
        public static readonly DependencyProperty TagRefOpenCommandProperty = DependencyProperty.Register(
         nameof(TagRefOpenCommand),
         typeof(ICommand),
         typeof(TagInstanceTreeview),
         new PropertyMetadata());

        public ICommand TagRefOpenCommand
        {
            get => (ICommand)GetValue(TagRefOpenCommandProperty);
            set => SetValue(TagRefOpenCommandProperty, value);
        }

        public static readonly DependencyProperty TagRefGenOpenCommandProperty = DependencyProperty.Register(
         nameof(TagRefGenOpenCommand),
         typeof(ICommand),
         typeof(TagInstanceTreeview),
         new PropertyMetadata());

        public ICommand TagRefGenOpenCommand
        {
            get => (ICommand)GetValue(TagRefGenOpenCommandProperty);
            set => SetValue(TagRefGenOpenCommandProperty, value);
        }

        public static readonly DependencyProperty TagToJsonCommandProperty = DependencyProperty.Register(
         nameof(TagToJsonCommand),
         typeof(ICommand),
         typeof(TagInstanceTreeview),
         new PropertyMetadata());

        public ICommand TagToJsonCommand
        {
            get => (ICommand)GetValue(TagToJsonCommandProperty);
            set => SetValue(TagToJsonCommandProperty, value);
        }
        
        public static readonly DependencyProperty TagGoToBinCommandProperty = DependencyProperty.Register(
         nameof(TagGoToBinCommand),
         typeof(ICommand),
         typeof(TagInstanceTreeview),
         new PropertyMetadata());

        public ICommand TagGoToBinCommand
        {
            get => (ICommand)GetValue(TagGoToBinCommandProperty);
            set => SetValue(TagGoToBinCommandProperty, value);
        }
        
        public static readonly DependencyProperty TagGoToTemplateCommandProperty = DependencyProperty.Register(
         nameof(TagGoToTemplateCommand),
         typeof(ICommand),
         typeof(TagInstanceTreeview),
         new PropertyMetadata());

        public ICommand TagGoToTemplateCommand
        {
            get => (ICommand)GetValue(TagGoToTemplateCommandProperty);
            set => SetValue(TagGoToTemplateCommandProperty, value);
        }

        public TagInstanceTreeview()
        {
            InitializeComponent();
        }

        private void TagRefButton_Click(object sender, RoutedEventArgs e)
        {
            TagRef tagRef = (TagRef)((sender as Button)?.DataContext);
            if (tagRef.Ref_id_int != -1)
                TagRefOpenCommand?.Execute(tagRef);
        }

        private void MenuItem_ExportToJsonClick(object sender, RoutedEventArgs e)
        {
            MenuItem temp = sender as MenuItem;
            if (temp == null || !(temp.DataContext is TagInstance))
                return;
            TagToJsonCommand?.Execute(temp.DataContext);
        }
        private void MenuItem_GoToBinClick(object sender, RoutedEventArgs e)
        {
            MenuItem temp = sender as MenuItem;
            if (temp == null || !(temp.DataContext is TagInstance))
                return;
            TagGoToBinCommand?.Execute(temp.DataContext);
        }
         private void MenuItem_GoToTemplateClick(object sender, RoutedEventArgs e)
        {
            MenuItem temp = sender as MenuItem;
            if (temp == null || !(temp.DataContext is TagInstance))
                return;
            TagGoToTemplateCommand?.Execute(temp.DataContext);
        }

        private void TagRefGenButton_Click(object sender, RoutedEventArgs e)
        {
            TagRef tagRef = (TagRef)((sender as Button)?.DataContext);
            if (tagRef.Ref_id_int != -1)
                TagRefGenOpenCommand?.Execute(tagRef);
        }
    }
}
