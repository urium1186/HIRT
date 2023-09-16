using LibHIRT.TagReader.Headers;
using System.Windows;
using System.Windows.Controls;

namespace HaloInfiniteResearchTools.Controls
{
    /// <summary>
    /// Interaction logic for TagFileViewer.xaml
    /// </summary>
    public partial class TagFileViewer : UserControl
    {
        public TagFile TagFile_file { get; set; }
        public TagFileViewer()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            /*if (e.NewValue != null ) {
                if (e.NewValue is TagFile) { 
                    TagFile_file = (TagFile)e.NewValue; 
                }else if (e.NewValue is GenericViewModel)
                {

                }
            }*/
        }


    }
}
