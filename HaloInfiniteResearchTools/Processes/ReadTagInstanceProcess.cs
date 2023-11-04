using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.TagReader;
using LibHIRT.TagReader.Common;
using LibHIRT.TagReader.RuntimeViewer;
using System;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    public class ReadTagInstanceProcess : ProcessBase
    {
        private ITagParseControl tagParse;

        private bool forceReload = false;

        private IHIRTFile _file;
        public event EventHandler<ITagInstance> OnInstanceLoadEvent;

        public ReadTagInstanceProcess(IHIRTFile file)
        {
            _file = file;
        }

        public ITagParseControl TagParse { get => tagParse; set => tagParse = value; }
        public bool ForceReload { get => forceReload; set => forceReload = value; }

        protected override async Task OnExecuting()
        {
            if (_file is SSpaceFile)
            {
                SSpaceFile file = (SSpaceFile)_file;
                tagParse = file.Deserialized(null, forceReload, _onDeserialized: OnInstanceLoadEvent)?.TagParse;
                //tagParse = new TagParseControl(file.Path_string, file.TagGroup, null, file.GetStream());
                //tagParse.OnInstanceLoadEvent += OnInstanceLoadEvent;
                //TagInstance.OnInstanceLoadEvent += TagParse_OnInstanceLoadEvent;
                //tagParse.readFile();
                //TagInstance.OnInstanceLoadEvent -= TagParse_OnInstanceLoadEvent;
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
            else if (_file is TagStructMemFile)
            {
                TagStructMemFile _fileMem = (TagStructMemFile)_file;
                if (HIFileContext.RuntimeTagLoader.checkLoadTagInstance(_fileMem.ObjectId))
                {
                    tagParse = new TagParseControlMem(_fileMem.TagGroup);
                    TagInstance.OnInstanceLoadEvent += TagParse_OnInstanceLoadEvent;
                    tagParse.readFile();
                    TagInstance.OnInstanceLoadEvent -= TagParse_OnInstanceLoadEvent;
                    /*_tagRoot.Add(tagParse.RootTagInst);
                    _tagRootModel.Add(new TagInstanceModel(tagParse.RootTagInst));
                    JsonFile = tagParse.RootTagInst.ToJson();
                    string temp = _fileMem._tagGroup;
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
