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

        #region Variables
        public XmlWriter textWriter;
        private Mem m = new Mem();

        private long startAddress = 0;
        private int tagCount = 0;
        private string outDIR = "";
        private TagStructsDumper structsDumper = new TagStructsDumper();
        /*
                private HashSet<int> unique_items_1 = new HashSet<int>();
                private HashSet<int> unique_items_2 = new HashSet<int>();
                private HashSet<int> unique_items_4 = new HashSet<int>();
                private HashSet<int> unique_items_5 = new HashSet<int>();
                private HashSet<int> unique_items_6 = new HashSet<int>();

                private HashSet<int> unique_items_8 = new HashSet<int>();        
                private HashSet<int> unique_items_9 = new HashSet<int>();
        */
        #endregion
        #region Control Buttons
        // Window Control Button Functions
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            //SystemCommands.MinimizeWindow(this);
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            //SystemCommands.CloseWindow(this);
        }

        private void Move_Window(object sender, MouseButtonEventArgs e)
        {
            //DragMove();
        }
        #endregion

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

        private async void Scan()
        {
            SetStatus("Scanning for starting address...");

            // await AoBScan();
            startAddress = 2178283012096;
            Console.WriteLine(startAddress.ToString());
            if (startAddress != 0)
            {
                SetStatus("Address Found: " + startAddress.ToString("X"));

                int warnings = 0;
                long curAddress = startAddress;
                bool scanning = true;

                while (scanning)
                {
                    if (m.ReadInt((curAddress + 80).ToString("X")) == 257)
                    {
                        tagCount++;
                        curAddress += 88;
                        warnings = 0;
                    }
                    else
                    {
                        warnings++;
                        curAddress += 88;
                    }

                    if (warnings > 3)
                    {
                        scanning = false;
                    }
                }

                SetStatus("Found " + tagCount + " tag structs!");
                structsDumper.M = m;
                structsDumper.OutDIR = outDIR;
                structsDumper.StartAddress = startAddress;
                structsDumper.DumpStructs();
                structsDumper.printSaveLogUniqueStr();
                SetStatus("Done!");
            }
        }

        private async Task AoBScan()
        {
            long[] results = (await m.AoBScan("?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 53 62 6F 47 67 61 54 61", true, false)).ToArray();
            startAddress = results[0];
        }

        public void SetStatus(string message)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                //statusText.Text = message;
            }));
        }
        #endregion

    }
}
