using HaloInfiniteResearchTools.ViewModels;
using LibHIRT.TagReader.Dumper;
using Memory;
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
using System.Text.RegularExpressions;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Xml;
using System.Text.RegularExpressions;

namespace HaloInfiniteResearchTools.Views
{
    /// <summary>
    /// Interaction logic for TagStructsDumperView.xaml
    /// </summary>
    public partial class TagStructsDumperView : View<TagStructsDumperViewModel>
    {
        public TagStructsDumperView()
        {
            InitializeComponent();
        }

        #region Zs Changes
        private void DumpClick(object sender, RoutedEventArgs e)
        {
            /*outDIR = OutPath.Text;
            if (outDIR.Length > 1)
            {
                if (m.OpenProcess("HaloInfinite.exe"))
                {
                    Scan();
                }
            }
            else
            {
                SetStatus("Please select a directory!");
            }
            */
        }

        private void OutputFolderClick(object sender, RoutedEventArgs e)
        {/*
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutPath.Text = dialog.SelectedPath;
            }
            */
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

       
        #endregion

    }
}
