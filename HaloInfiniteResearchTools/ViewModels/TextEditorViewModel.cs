﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Extensions.DependencyInjection;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;

namespace HaloInfiniteResearchTools.ViewModels
{

  /*[AcceptsFileType( typeof( GenericTextFile ) )]
  [AcceptsFileType( typeof( TextureDefinitionFile ) )]
  [AcceptsFileType( typeof( PresetFile ) )]
  [AcceptsFileType( typeof( ShaderDefinitionFile ) )]
  [AcceptsFileType( typeof( ScriptingFile ) )]
  [AcceptsFileType( typeof( ObjectClassFile ) )]
  [AcceptsFileType( typeof( ShaderCodeFile ) )]*/
  public class TextEditorViewModel : ViewModel, IDisposeWithView
  {

    #region Data Members

    private readonly ISSpaceFile _file;
    private readonly IFileDialogService _dialogService;

    #endregion

    #region Properties

    public TextDocument Document { get; set; }

    public ICommand ExportCommand { get; }

    #endregion

    #region Constructor

    public TextEditorViewModel( IServiceProvider serviceProvider, ISSpaceFile file )
      : base( serviceProvider )
    {
      _file = file;
      _dialogService = serviceProvider.GetRequiredService<IFileDialogService>();

      ExportCommand = new AsyncCommand( ExportFile );
    }

    #endregion

    #region Overides

    protected override async Task OnInitializing()
    {
      try
      {
        Document = await LoadDocument( _file );
      }
      catch ( Exception ex )
      {
        await ShowExceptionModal( ex );
      }
    }

    #endregion

    #region Private Methods

    private async Task<TextDocument> LoadDocument( ISSpaceFile file )
    {
      var fileStream = file.GetStream();

      try
      {
        fileStream.AcquireLock();
        using ( var reader = new StreamReader( fileStream, leaveOpen: true ) )
          return new TextDocument( await reader.ReadToEndAsync() );
      }
      finally { fileStream.ReleaseLock(); }
    }

    private async Task ExportFile()
    {
      var fName = _file.Name;
      var path = GetPreferences().DefaultExportPath;
      if ( !string.IsNullOrWhiteSpace( path ) && Directory.Exists( path ) )
        fName = Path.Combine( path, fName );

      var outputFile = await _dialogService.BrowseForSaveFile(
        title: "Export File",
        defaultFileName: fName,
        filter: $"Saber File|*{Path.GetExtension( fName )}" );

      if ( string.IsNullOrWhiteSpace( outputFile ) )
        return;

      using ( var fs = File.Create( outputFile ) )
      using ( var writer = new StreamWriter( fs ) )
      {
        Document.WriteTextTo( writer );

        await writer.FlushAsync();
      }
    }

    #endregion

  }

}
