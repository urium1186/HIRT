using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using LibHIRT.TagReader.Common;
using LibHIRT.TagReader.Headers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WSF.IDs;
using static LibHIRT.TagReader.TagLayouts;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(GenericFile))]
    public class GenericViewModel : ViewModel, IDisposeWithView
    {
        private SSpaceFile _file;
        private TagStructMem _fileMem;
        private string _jsonFile;
        private List<TagInstance> _tagRoot = new List<TagInstance>();
        private List<TagFile> _tagFile = new List<TagFile>();
        private List<TagInstanceModel> _tagRootModel = new List<TagInstanceModel>();
        private TagParseControl tagParse;
        private readonly IHIFileContext _fileContext;
        private readonly ITabService _tabService;
        public ICommand OpenFileTabCommand { get; }

        public string JsonFile { get;  set ; }
        public Stream FileStream { get;  set ; }
        public string XmlPath { get;  set ; }

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
        public List<TagInstance> TagRoot { get => _tagRoot; set => _tagRoot = value; }
        public List<TagFile> TagFile { get => _tagFile; set => _tagFile = value; }

        public GenericViewModel(IServiceProvider serviceProvider, SSpaceFile file) : base(serviceProvider)
        {
            _file = file;
            _fileMem = null;
            OpenFileTabCommand = new AsyncCommand<TagRef>(OpenFileTab);
            _fileContext = serviceProvider.GetRequiredService<IHIFileContext>();
            _tabService = serviceProvider.GetService<ITabService>();
        }
         public GenericViewModel(IServiceProvider serviceProvider, TagStructMem fileMem) : base(serviceProvider)
        {
            _file = null;
            _fileMem = fileMem;
            OpenFileTabCommand = new AsyncCommand<TagRef>(OpenFileTab);
            _fileContext = serviceProvider.GetRequiredService<IHIFileContext>();
            _tabService = serviceProvider.GetService<ITabService>();
        }

        protected override async Task OnInitializing()
        {
            if (_file != null)
            {
                tagParse = new TagParseControl(_file.Path_string, _file.TagGroup, null, _file.GetStream());
                TagInstance.OnInstanceLoadEvent += TagParse_OnInstanceLoadEvent;
                tagParse.readFile();
                TagInstance.OnInstanceLoadEvent -= TagParse_OnInstanceLoadEvent;
                _tagRoot.Add(tagParse.RootTagInst);
                if (tagParse.TagFile != null) { 
                    _tagFile.Add(tagParse.TagFile); 
                }   
                _tagRootModel.Add(new TagInstanceModel(tagParse.RootTagInst));
                JsonFile = tagParse.RootTagInst.ToJson();
                FileStream = _file.GetStream();
                XmlPath = _file.GetTagXmlTempaltePath();
            }
            else if (_fileMem != null){
                if (HIFileContext.RuntimeTagLoader.checkLoadTagInstance(_fileMem.ObjectId))
                {
                    tagParse = new TagParseControl("", _fileMem.TagGroup, null, null);
                    TagInstance.OnInstanceLoadEvent += TagParse_OnInstanceLoadEvent;
                    tagParse.readOnMem(_fileMem.TagData, HIFileContext.RuntimeTagLoader.M);
                    TagInstance.OnInstanceLoadEvent -= TagParse_OnInstanceLoadEvent;
                    _tagRoot.Add(tagParse.RootTagInst);
                    _tagRootModel.Add(new TagInstanceModel(tagParse.RootTagInst));
                    JsonFile = tagParse.RootTagInst.ToJson();
                    string temp = _fileMem.TagGroup;
                    XmlPath = TagXmlParse.GetXmlPath(ref temp);
                }

            }
            

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
            }
        }
    }
}
