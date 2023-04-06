using HaloInfiniteResearchTools.Models;
using LibHIRT.Files;
using LibHIRT.TagReader.Headers;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LibHIRT.TagReader.TagLayouts;
using LibHIRT.Files.Base;
using LibHIRT.TagReader.Common;

namespace HaloInfiniteResearchTools.Processes
{
    public class ReadTagInstanceProcess : ProcessBase
    {
        private TagParseControl tagParse;

        private IHIRTFile _file;
        public event EventHandler<ITagInstance> OnInstanceLoadEvent;

        public ReadTagInstanceProcess(IHIRTFile file)
        {
            _file = file;
        }

        public TagParseControl TagParse { get => tagParse; set => tagParse = value; }

        protected override async Task OnExecuting()
        {
            if (_file is SSpaceFile)
            {
                SSpaceFile file = (SSpaceFile)_file;  
                tagParse = new TagParseControl(file.Path_string, file.TagGroup, null, file.GetStream());
                TagInstance.OnInstanceLoadEvent += TagParse_OnInstanceLoadEvent;
                tagParse.readFile();
                TagInstance.OnInstanceLoadEvent -= TagParse_OnInstanceLoadEvent;
                /*_tagRoot.Add(tagParse.RootTagInst);
                if (tagParse.TagFile != null)
                {
                    _tagFile.Add(tagParse.TagFile);
                }*/
                /*_tagRootModel.Add(new TagInstanceModel(tagParse.RootTagInst));
                JsonFile = tagParse.RootTagInst.ToJson();
                FileStream = _file.GetStream();
                XmlPath = _file.GetTagXmlTempaltePath();*/
            }
            else if (_file is TagStructMem)
            {
                TagStructMem _fileMem = (TagStructMem)_file;
                if (HIFileContext.RuntimeTagLoader.checkLoadTagInstance(_fileMem.ObjectId))
                {
                    tagParse = new TagParseControl("", _fileMem.TagGroup, null, null);
                    TagInstance.OnInstanceLoadEvent += TagParse_OnInstanceLoadEvent;
                    tagParse.readOnMem(_fileMem.TagData, HIFileContext.RuntimeTagLoader.M);
                    TagInstance.OnInstanceLoadEvent -= TagParse_OnInstanceLoadEvent;
                    /*_tagRoot.Add(tagParse.RootTagInst);
                    _tagRootModel.Add(new TagInstanceModel(tagParse.RootTagInst));
                    JsonFile = tagParse.RootTagInst.ToJson();
                    string temp = _fileMem.TagGroup;
                    XmlPath = TagXmlParse.GetXmlPath(ref temp);*/
                }

            }
           
        }

        private void TagParse_OnInstanceLoadEvent(object? sender, ITagInstance e)
        {
            OnInstanceLoadEvent?.Invoke(this, e);
        }
    }
}
