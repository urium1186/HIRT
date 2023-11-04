using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using HaloInfiniteResearchTools.Views;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HaloInfiniteResearchTools.ViewModels
{

    [AcceptsFileType(typeof(PictureFile))]
    public class TextureViewModel : ViewModel, IDisposeWithView
    {

        #region Data Members

        private readonly IHIFileContext _fileContext;
        private readonly ITextureConversionService _textureService;
        private readonly ITabService _tabService;

        private int[] _options = null;
        private int _optionSelected = 0;

        private readonly PictureFile _file;

        #endregion

        #region Properties

        public TextureModel Texture { get; set; }
        public bool ShowOptSelector { get; set; }

        public ICommand OpenTextureDefinitionCommand { get; }
        public ICommand ExportTextureCommand { get; }
        public int[] Opciones { get => _options; set => _options = value; }
        public int OpcionSeleccionada { get => _optionSelected; set {
                if (_optionSelected != value)
                {
                    _optionSelected = value;
                    changeTextureView();
                }
                

            }
        }

        #endregion

        #region Constructor

        public TextureViewModel(IServiceProvider serviceProvider, PictureFile file)
          : base(serviceProvider)
        {
            _file = file;

            _fileContext = HIFileContext.Instance;
            _textureService = ServiceProvider.GetService<ITextureConversionService>();
            _tabService = ServiceProvider.GetService<ITabService>();

            OpenTextureDefinitionCommand = new AsyncCommand(OpenTextureDefinitionFile);
            ExportTextureCommand = new AsyncCommand(ExportTexture);
        }

        #endregion

        #region Overrides

        protected override async Task OnInitializing()
        {
            using (var progress = ShowProgress())
            {
                progress.IsIndeterminate = true;
                progress.Status = "Loading Texture";

                var previewQuality = GetPreferences().TextureViewerOptions.PreviewQuality;
                Texture = await _textureService.LoadTexture(_file, previewQuality);
                Texture.SelectedMip = Texture.SelectedFace.MipMaps[_file.MaxResMipMapIndex];
                _options = new int[_file.BitmapsCount];
                ShowOptSelector = _file.BitmapsCount > 1;
                for (int i = 0; i < _file.BitmapsCount; i++)
                {
                    _options[i] = i;
                }
                OpcionSeleccionada = 0;
            }
        }

        protected async void changeTextureView() {
            using (var progress = ShowProgress())
            {
                progress.IsIndeterminate = true;
                progress.Status = "Loading Texture";

                var previewQuality = GetPreferences().TextureViewerOptions.PreviewQuality;
                _file.CurrentBitmapIndex = OpcionSeleccionada;
                Texture = await _textureService.LoadTexture(_file, previewQuality);
                Texture.SelectedMip = Texture.SelectedFace.MipMaps[_file.MaxResMipMapIndex];
            }
        }

        protected override void OnDisposing()
        {
            Texture?.Dispose();
            base.OnDisposing();

            GCHelper.ForceCollect();
        }

        #endregion

        #region Private Methods

        private Task OpenTextureDefinitionFile()
        {
            var tdFile = _fileContext.GetFiles(Path.ChangeExtension(_file.Name, ".td")).First();
            if (tdFile is null)
                return ShowMessageModal("File Not Found", "Could not find a texture definition for this file.");

            _tabService.CreateTabForFile(tdFile, out _);
            return Task.CompletedTask;
        }

        private async Task ExportTexture()
        {
            var result = await ShowViewModal<TextureExportOptionsView>();
            if (!(result is TextureExportOptionsModel options))
                return;

            var exportProcess = new ExportTextureProcess(_file, options);
            await RunProcess(exportProcess);
        }

        #endregion

    }

}
