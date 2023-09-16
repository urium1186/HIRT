using LibHIRT.TagReader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public static readonly DependencyProperty RenderGeomGenOpenCommandProperty = DependencyProperty.Register(
         nameof(RenderGeomGenOpenCommand),
         typeof(ICommand),
         typeof(TagInstanceTreeview),
         new PropertyMetadata());

        public ICommand RenderGeomGenOpenCommand
        {
            get => (ICommand)GetValue(RenderGeomGenOpenCommandProperty);
            set => SetValue(RenderGeomGenOpenCommandProperty, value);
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

        public static readonly DependencyProperty WriteToCommandProperty = DependencyProperty.Register(
         nameof(WriteToCommand),
         typeof(ICommand),
         typeof(TagInstanceTreeview),
         new PropertyMetadata());

        public ICommand WriteToCommand
        {
            get => (ICommand)GetValue(WriteToCommandProperty);
            set => SetValue(WriteToCommandProperty, value);
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

        private void MenuItem_WriteToClick(object sender, RoutedEventArgs e)
        {
            MenuItem temp = sender as MenuItem;
            if (temp == null || !(temp.DataContext is TagInstance))
                return;
            WriteToCommand?.Execute(temp.DataContext);
        }

        private void MenuItem_GoToTemplateClick(object sender, RoutedEventArgs e)
        {
            MenuItem temp = sender as MenuItem;
            if (temp == null || !(temp.DataContext is TagInstance))
                return;
            TagGoToTemplateCommand?.Execute(temp.DataContext);
        }



        private void MenuItem_ToggleEditClick(object sender, RoutedEventArgs e)
        {
            MenuItem temp = sender as MenuItem;
            if (temp == null || !(temp.DataContext is AtomicTagInstace))
                return;
            (temp.DataContext as AtomicTagInstace).NoAllowEdit = !(temp.DataContext as AtomicTagInstace).NoAllowEdit;//=="False"?"True":"False"
        }

        private void TagRefGenButton_Click(object sender, RoutedEventArgs e)
        {
            TagRef tagRef = (TagRef)((sender as Button)?.DataContext);
            if (tagRef.Ref_id_int != -1)
                TagRefGenOpenCommand?.Execute(tagRef);
        }

        private void RenderGeometryTag_Click(object sender, RoutedEventArgs e)
        {
            RenderGeometryTag render_geom = (RenderGeometryTag)((sender as Button)?.DataContext);
            if (render_geom != null)
                RenderGeomGenOpenCommand?.Execute(render_geom);
        }

    }
}
