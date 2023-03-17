using HaloInfiniteResearchTools.Common;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Models
{
   
    public class PreferencesModel : ObservableObject
    {
        

        #region Properties

        public static PreferencesModel Default
        {
            get
            {
                return new PreferencesModel
                {
                    ModelExportOptions = ModelExportOptionsModel.Default,
                    ModelViewerOptions = ModelViewerOptionsModel.Default,
                    TextureExportOptions = TextureExportOptionsModel.Default,
                    TextureViewerOptions = TextureViewerOptionsModel.Default,
                    TagStructsDumperOptions = TagStructsDumperOptionsModel.Default,
                    TagReaderOptionsModel = TagReaderOptionsModel.Default
                };
            }
        }

        [DefaultValue(false)]
        public bool LoadH2ADirectoryOnStartup { get; set; }

        public string H2ADirectoryPath { get; set; }

        [OnChangedMethod(nameof(SetGlobalDefaults))]
        public string DefaultExportPath { get; set; }

        [OnChangedMethod(nameof(SetGlobalDefaults))]
        public ModelExportOptionsModel ModelExportOptions { get; set; }
        
        [OnChangedMethod(nameof(SetGlobalDefaults))]
        public TagReaderOptionsModel TagReaderOptionsModel { get; set; }
        
        [OnChangedMethod(nameof(SetGlobalDefaults))]
        public TextureExportOptionsModel TextureExportOptions { get; set; }

        [OnChangedMethod(nameof(SetGlobalDefaults))]
        public TagStructsDumperOptionsModel TagStructsDumperOptions { get; set; }
        
        public ModelViewerOptionsModel ModelViewerOptions { get; set; }
        
        public TextureViewerOptionsModel TextureViewerOptions { get; set; }
        
        #endregion

        #region Constructor

        public PreferencesModel()
        {
        }

        #endregion

        #region Private Methods

        private void SetGlobalDefaults()
        {
            if (ModelExportOptions != null)
                ModelExportOptions.OutputPath = DefaultExportPath;
            if (TextureExportOptions != null)
                TextureExportOptions.OutputPath = DefaultExportPath;

        }

        #endregion

    }
}
