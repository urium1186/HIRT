﻿using HaloInfiniteResearchTools.ViewModels;
using LibHIRT.TagReader.Headers;
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
