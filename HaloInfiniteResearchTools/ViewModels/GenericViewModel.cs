using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ControlModel;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.Services.Abstract;
using HaloInfiniteResearchTools.UI.Modals;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using HaloInfiniteResearchTools.Views;
using LibHIRT.Common;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using LibHIRT.TagReader.Common;
using LibHIRT.TagReader.Headers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static LibHIRT.TagReader.TagLayouts;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(GenericFile))]
    public class GenericViewModel : ViewModel, IDisposeWithView
    {
        private SSpaceFile _file;
        private TagStructMem _fileMem;
        private string _jsonFile;
        private ObservableCollection<TagInstance> _tagRoot;
        private List<TagFile> _tagFile = new List<TagFile>();
        private List<TagInstanceModel> _tagRootModel = new List<TagInstanceModel>();
        private TagParseControl tagParse;
        private readonly IHIFileContext _fileContext;
        private readonly ITabService _tabService;
        private readonly IMeshIdentifierService _meshIdentifierService;

        private readonly Model3DViewerControlModel _dViewerControlModel;

        public int SelectedTabIndex { get; set; }
        public int RenderGeomViewerEnable { get; set; }
        public long PositionOnCurrentStream { get; set; }

        public ICommand OpenFileTabCommand { get; }
        public ICommand OpenGenFileTabCommand { get; }
        public ICommand TagInstanceExportJsonCommand { get; }
        public ICommand TagGoToBinCommand { get; }
        public ICommand WriteToCommand { get; }
        public ICommand WriteTagCommand { get; }
        public ICommand TagGoToTemplateCommand { get; }
        public ICommand RenderGeomGenOpenCommand { get; }
        public ICommand ExportModelCommand { get; }

        public string JsonFile { get;  set ; }
        public Stream FileStream { get;  set ; }
        public string XmlPath { get;  set ; }

        public ModelViewerOptionsModel Options { get; set; }

        public string CustoJsonFilemerName
        {
            get
            {
                return this._jsonFile;
            }

            set
            {
                if (value != this._jsonFile)
                {
                    this._jsonFile = value;
                }
            }
        }

        public List<TagInstanceModel> TagRootModel { get => _tagRootModel; set => _tagRootModel = value; }
        public ObservableCollection<TagInstance> TagRoot { get => _tagRoot; set => _tagRoot = value; }
        public List<TagFile> TagFile { get => _tagFile; set => _tagFile = value; }

        public IMeshIdentifierService MeshIdentifierService => _meshIdentifierService;

        public Model3DViewerControlModel DViewerControlModel => _dViewerControlModel;

        public GenericViewModel(IServiceProvider serviceProvider, SSpaceFile file) : base(serviceProvider)
        {
            _dViewerControlModel = new Model3DViewerControlModel(serviceProvider, file);
            _file = file;
            RenderGeomViewerEnable = -1;
            PositionOnCurrentStream = 0;
            _fileMem = null;
            _tagRoot = new ObservableCollection<TagInstance>();
            OpenFileTabCommand = new AsyncCommand<TagRef>(OpenFileTab);
            OpenGenFileTabCommand = new AsyncCommand<TagRef>(OpenGenFileTab);
            TagInstanceExportJsonCommand = new AsyncCommand<TagInstance>(TagInstanceExportJson);
            TagGoToBinCommand = new AsyncCommand<TagInstance>(TagGoToBin);
            WriteToCommand = new AsyncCommand<TagInstance>(WriteTo);
            WriteTagCommand = new AsyncCommand(WriteTag);
            TagGoToTemplateCommand = new AsyncCommand<TagInstance>(TagGoToTemplate);
            RenderGeomGenOpenCommand = new AsyncCommand<RenderGeometryTag>(RenderGeomGenOpen);
            //ExportModelCommand = new AsyncCommand(ExportModel);
            _fileContext = serviceProvider.GetRequiredService<IHIFileContext>();
            _tabService = serviceProvider.GetService<ITabService>();
            _meshIdentifierService = ServiceProvider.GetRequiredService<IMeshIdentifierService>();
            

        }

        private async Task ExportModel()
        {
            Tuple<ModelExportOptionsModel, TextureExportOptionsModel> result = (Tuple<ModelExportOptionsModel, TextureExportOptionsModel>)await ShowViewModal<ModelExportOptionsView>();
            await _dViewerControlModel.ExportModel(result);
        }

        private async Task WriteTo(TagInstance arg)
        {
            if (arg == null || !(arg is FlagGroup))
                return;
            arg.WriteIn(FileStream);
        }

        private async Task RenderGeomGenOpen(RenderGeometryTag arg)
        {
            if (arg == null)
                return;
            _dViewerControlModel.RenderGeometryTag = arg;
            _dViewerControlModel.ExtExportModelCommand = new AsyncCommand(ExportModel);
            await _dViewerControlModel.Initialize();
            
            RenderGeomViewerEnable = 1; 
            SelectedTabIndex = 4;

            
        }

        private  async Task TagGoToBin(TagInstance arg)
        {
            if (arg == null)
                return;
            
            PositionOnCurrentStream = arg.InFileOffset;
            /*this.FileStream.Seek(2055288, SeekOrigin.Begin);
            this.FileStream.WriteByte(0x78);
            this.FileStream.WriteByte(0x18);*/
            SelectedTabIndex = 1;
        } 
        
        private  async Task WriteTag()
        {
            try
            {
                if (_file != null)
                {
                    ModuleFile temp = _file.Parent as ModuleFile;
                    if (temp != null)
                    {
                        temp.WriteTag(_file);
                    }
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex);
            }
            
        }
        private  async Task TagGoToTemplate(TagInstance arg)
        {
            if (arg == null)
                return;
            SelectedTabIndex = 2;
        }

        private async Task TagInstanceExportJson(TagInstance arg)
        {
            if (arg == null)
                return;
            
            string result = arg.ToJson();
            if (result == null) return;
            string file_name = _file != null ? _file.Name : _fileMem.TagFullName;
            var te = arg.TagDef.xmlPath.Item2.Replace("#document","").Replace("\\","__");
            file_name = file_name.Replace(".", "_") + ".json";
            if (string.IsNullOrEmpty(GetPreferences().DefaultExportPath))
                return;
            string file_path = GetPreferences().DefaultExportPath+ "\\"+ file_name;
            System.IO.File.WriteAllText(file_path, result);   
        }

        public GenericViewModel(IServiceProvider serviceProvider, TagStructMem fileMem) : base(serviceProvider)
        {
            _file = null;
            _fileMem = fileMem;
            OpenFileTabCommand = new AsyncCommand<TagRef>(OpenFileTab);
            OpenGenFileTabCommand = new AsyncCommand<TagRef>(OpenGenFileTab);
            _fileContext = serviceProvider.GetRequiredService<IHIFileContext>();
            _tabService = serviceProvider.GetService<ITabService>();
            _tagRoot = new ObservableCollection<TagInstance>();
            _tagRootModel = new List<TagInstanceModel>();
        }

        protected override async Task OnInitializing()
        {
            if (_file != null)
            {
                var process = new ReadTagInstanceProcess(_file);

                process.OnInstanceLoadEvent += TagParse_OnInstanceLoadEvent;
                //await RunProcess(process);

                var modal = ServiceProvider.GetService<ProgressModal>();
                modal.DataContext = process;

                using (modal)
                {
                    Modals.Add(modal);
                    modal.Show();
                    IsBusy = true;

                    await Task.Factory.StartNew(process.Execute, TaskCreationOptions.LongRunning);
                    await process.CompletionTask;

                    await modal.Hide();
                    Modals.Remove(modal);
                    IsBusy = false;
                }

                var statusList = process.StatusList;
                if (statusList.HasErrors || statusList.HasWarnings)
                    await ShowStatusListModal(statusList);


                tagParse = process.TagParse;
                
                if (tagParse.TagFile != null) { 
                    _tagFile.Add(tagParse.TagFile); 
                }
                if (tagParse.RootTagInst != null)
                {
                    _tagRoot.Add(tagParse.RootTagInst);
                    _tagRootModel.Add(new TagInstanceModel(tagParse.RootTagInst));
                }
                
                    
                FileStream = _file.GetStream();
                if (_file.TagGroup != "����")
                    XmlPath = _file.GetTagXmlTempaltePath();
                else
                {
                    if (tagParse.TagFile != null)
                    {
                        string hash = BitConverter.ToString(BitConverter.GetBytes(tagParse.TagFile.TagHeader.TagFileHeaderInst.TypeHash)).Replace("-", "");
                        var tempTaglay = tagParse.getSubTaglayoutFrom(_file.FileMemDescriptor.ParentOffResourceRef?.TagGroup, hash);
                        if (tempTaglay != null)
                        {
                            tagParse.readFile(tempTaglay, tagParse.TagFile);
                            if (tagParse.RootTagInst != null)
                            {
                                _tagRoot.Add(tagParse.RootTagInst);
                                _tagRootModel.Add(new TagInstanceModel(tagParse.RootTagInst));
                            }
                        }
                    }
                }
            }
            else if (_fileMem != null){
                if (HIFileContext.RuntimeTagLoader.checkLoadTagInstance(_fileMem.ObjectId))
                {
                    var process = new ReadTagInstanceProcess(_fileMem);

                    //process.Completed += OpenFilesProcess_Completed;
                    //await RunProcess(process);

                    var modal = ServiceProvider.GetService<ProgressModal>();
                    modal.DataContext = process;

                    using (modal)
                    {
                        Modals.Add(modal);
                        modal.Show();
                        IsBusy = true;

                        await Task.Factory.StartNew(process.Execute, TaskCreationOptions.LongRunning);
                        await process.CompletionTask;

                        await modal.Hide();
                        Modals.Remove(modal);
                        IsBusy = false;
                    }

                    var statusList = process.StatusList;
                    if (statusList.HasErrors || statusList.HasWarnings)
                        await ShowStatusListModal(statusList);


                    tagParse = process.TagParse;
                    _tagRoot.Add(tagParse.RootTagInst);
                    _tagRootModel.Add(new TagInstanceModel(tagParse.RootTagInst));
                    FileStream = tagParse.MemoStream;
                    string temp = _fileMem.TagGroup;
                    XmlPath = TagXmlParse.GetXmlPath(ref temp);
                }

            }
            

        }

        protected PreferencesModel GetPreferences()
        {
            var prefService = ServiceProvider.GetRequiredService<IPreferencesService>();
            return prefService.Preferences;
        }

        private async Task OpenFileTab(TagRef tagRef)
        {
            if (_file != null )
            {
                ISSpaceFile file = null;
                if (_file.Parent != null)
                {
                    file = (SSpaceFile)(_file.Parent as ModuleFile).GetFileByGlobalId((int)tagRef.Ref_id_int);
                    if (file == null)
                    {
                        file = (SSpaceFile)(_file.Parent as ModuleFile).GetFileByGlobalId((int)tagRef.Ref_id_center_int);
                        if (file == null)
                        {
                            file = (SSpaceFile)(_file.Parent as ModuleFile).GetFileByGlobalId((int)tagRef.Ref_id_sub_int);
                        }

                    }
                    
                }
                else {
                    HIFileContext.FilesGlobalIdLockUp.TryGetValue((int)tagRef.Ref_id_int,out file);

                }
                if (file == null)
                {
                    return;
                }

                if (!_tabService.CreateTabForFile(file, out _))
                {
                    var fileExt = Path.GetExtension(file.Name);
                    await ShowMessageModal(
                      title: "Unsupported File Type",
                      message: $"We can't open {fileExt} files yet.");

                    return;
                }
            }
            else if (_fileMem != null) {
                
                if (HIFileContext.RuntimeTagLoader.TagsList.ContainsKey(tagRef.Ref_id)) {
                    var file = HIFileContext.RuntimeTagLoader.TagsList[tagRef.Ref_id];
                    if (!_tabService.CreateTabForFile(file, out _))
                    {
                        var fileExt = Path.GetExtension(file.TagGroup);
                        await ShowMessageModal(
                          title: "Unsupported File Type",
                          message: $"We can't open {fileExt} files yet.");

                        return;
                    }
                }
            }
            
        }

        private async Task OpenGenFileTab(TagRef tagRef)
        {
            if (_file != null)
            {
                ISSpaceFile file = null;
                if (_file.Parent != null)
                {
                    file = (SSpaceFile)(_file.Parent as ModuleFile).GetFileByGlobalId((int)tagRef.Ref_id_int);
                    if (file == null)
                    {
                        file = (SSpaceFile)(_file.Parent as ModuleFile).GetFileByGlobalId((int)tagRef.Ref_id_center_int);
                        if (file == null)
                        {
                            file = (SSpaceFile)(_file.Parent as ModuleFile).GetFileByGlobalId((int)tagRef.Ref_id_sub_int);
                        }

                    }

                }
                else
                {
                    HIFileContext.FilesGlobalIdLockUp.TryGetValue((int)tagRef.Ref_id_int, out file);

                }
                if (file == null)
                {
                    return;
                }

                if (!_tabService.CreateTabForFile(file, out _, true))
                {
                    var fileExt = Path.GetExtension(file.Name);
                    await ShowMessageModal(
                      title: "Unsupported File Type",
                      message: $"We can't open {fileExt} files yet.");

                    return;
                }
            }
            else if (_fileMem != null)
            {

                if (HIFileContext.RuntimeTagLoader.TagsList.ContainsKey(tagRef.Ref_id))
                {
                    var file = HIFileContext.RuntimeTagLoader.TagsList[tagRef.Ref_id];
                    if (!_tabService.CreateTabForFile(file, out _, true))
                    {
                        var fileExt = Path.GetExtension(file.TagGroup);
                        await ShowMessageModal(
                          title: "Unsupported File Type",
                          message: $"We can't open {fileExt} files yet.");

                        return;
                    }
                }
            }

        }



        private void TagParse_OnInstanceLoadEvent(object? sender, ITagInstance e)
        {
            
            if (e != null) {
                switch (((TagInstance)e).TagDef.G)
                {
                    case "_37":
                        break;
                    default:
                        break;
                }
                string classHash = ((TagInstance)e).TagDef.E?["hash"].ToString();
                switch (classHash)
                {
                    case "9B6D4747FC4FE0CCB27AC58D817D912A":
                        decompileShaderAsync((TagInstance)e);
                        break; 
                    case "36C8F698C54393CD883131BA77BABD05":
                        
                        break;
                    default:
                        break;
                }
                if (((TagInstance)e).TagDef.N == "raw vertices") { 
                }
            }
        }


        async Task decompileShaderAsync(TagInstance ti) {
            try
            {
                var temp = ti["rootSignatureData"] as TagData;

                MemoryStream stream = new MemoryStream(temp?.ReadBuffer());
                ShaderByteCodeDecompileProcess process = new ShaderByteCodeDecompileProcess(temp?.ReadBuffer(),true);

                await Task.Factory.StartNew(process.Execute, TaskCreationOptions.LongRunning);
                await process.CompletionTask;

                //SharpDX.D3DCompiler.ShaderBytecode sb = SharpDX.D3DCompiler.ShaderBytecode.FromStream(stream);

                //string code = sb.Disassemble(
                /* SharpDX.D3DCompiler.DisassemblyFlags.EnableColorCode */
                //    SharpDX.D3DCompiler.DisassemblyFlags.EnableDefaultValuePrints
                /*| SharpDX.D3DCompiler.DisassemblyFlags.EnableInstructionCycle // this will cause an error, lol */
                //| SharpDX.D3DCompiler.DisassemblyFlags.EnableInstructionNumbering
                //    );


            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());

                
            }
        }
    }
}
