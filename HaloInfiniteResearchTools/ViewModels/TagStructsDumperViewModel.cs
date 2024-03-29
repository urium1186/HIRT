﻿using HaloInfiniteResearchTools.UI.Modals;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.Design.AxImporter;
using System.Windows.Data;
using System.Windows;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Common.Enumerations;
using System.IO;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    #region Constructor

    public class TagStructsDumperViewModel : ViewModel, IModalFooterButtons
    {
        public TagStructsDumperOptionsModel ModelOptions { get; set; }

        public bool IsValidPath { get; set; }

        public TagStructsDumperViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        #region Overrides

        protected override async Task OnInitializing()
        {
            ModelOptions = GetPreferences().TagStructsDumperOptions;

            var defaultExportPath = GetPreferences().DefaultExportPath;
            if (string.IsNullOrWhiteSpace(ModelOptions.OutputPath) && Directory.Exists(ModelOptions.OutputPath) && string.IsNullOrWhiteSpace(defaultExportPath) && Directory.Exists(defaultExportPath))
                ModelOptions.OutputPath = defaultExportPath;
        }

        #endregion


        #region IModalFooterButtons Members

        public IEnumerable<Button> GetFooterButtons()
        {
            yield return new Button { Content = "Cancel" };

            var exportBtn = new Button
            {
                Content = "Dump",
                Style = (Style)App.Current.FindResource("ColorfulFooterButtonStyle"),
                CommandParameter = ModelOptions
            };

            var exportBtnEnabledBinding = new Binding(nameof(IsValidPath));
            exportBtnEnabledBinding.Source = this;
            BindingOperations.SetBinding(exportBtn, Button.IsEnabledProperty, exportBtnEnabledBinding);

            yield return exportBtn;      
        }

        #endregion
    }

    #endregion
}
