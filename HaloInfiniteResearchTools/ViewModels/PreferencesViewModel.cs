using HaloInfiniteResearchTools.Common.Enumerations;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.UI.Modals;
using HelixToolkit.SharpDX.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HaloInfiniteResearchTools.ViewModels
{
    public class PreferencesViewModel : ViewModel, IModalFooterButtons
    {

        #region Properties

        public PreferencesModel Preferences { get; set; }

        public IReadOnlyList<FXAALevel> RenderFxaaLevels { get; set; }
        public IReadOnlyList<ModelFileFormat> ModelFileFormats { get; set; }
        public IReadOnlyList<TextureFileFormat> TextureFileFormats { get; set; }
        public IReadOnlyList<NormalMapFormat> NormalMapFormats { get; set; }

        #endregion

        #region Constructor

        public PreferencesViewModel(IServiceProvider serviceProvider)
          : base(serviceProvider)
        {
        }

        #endregion

        #region Overrides

        protected override async Task OnInitializing()
        {
            Preferences = GetPreferences();

            RenderFxaaLevels = Enum.GetValues<FXAALevel>();
            ModelFileFormats = Enum.GetValues<ModelFileFormat>();
            TextureFileFormats = Enum.GetValues<TextureFileFormat>();
            NormalMapFormats = Enum.GetValues<NormalMapFormat>();
            if (Preferences.TagReaderOptionsModel == null)
                Preferences.TagReaderOptionsModel = new TagReaderOptionsModel();
            if (Preferences.TagReaderOptionsModel.Paths == null)
                Preferences.TagReaderOptionsModel.Paths = new ObservableCollection<TagReaderPath>();
        }

        #endregion

        public void addPath(string path)
        {

        }

        public void removePath(TagReaderPath item)
        {
            if (Preferences.TagReaderOptionsModel.Paths != null && Preferences.TagReaderOptionsModel.Paths.Count > 1)
            {

                Preferences.TagReaderOptionsModel.Paths.Remove(item);
                if (item.Active == true && Preferences.TagReaderOptionsModel.Paths.Count > 0)
                    Preferences.TagReaderOptionsModel.Paths[0].Active = true;


            }
        }

        public IEnumerable<Button> GetFooterButtons()
        {
            yield return new Button { Content = "Cancel" };

            var exportBtn = new Button
            {
                Content = "Save",
                Style = (Style)App.Current.FindResource("ColorfulFooterButtonStyle"),
                CommandParameter = Preferences
            };
            /*
            var exportBtnEnabledBinding = new Binding(nameof(IsValidPath));
            exportBtnEnabledBinding.Source = this;
            BindingOperations.SetBinding(exportBtn, Button.IsEnabledProperty, exportBtnEnabledBinding);
            */
            yield return exportBtn;
        }
    }
}
