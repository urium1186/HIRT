using LibHIRT.Files;
using LibHIRT.Files.Base;
using LibHIRT.TagReader;
using LibHIRT.TagReader.Common;
using LibHIRT.TagReader.RuntimeViewer;
using System;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    public class ReadTagInstanceProcessV2 : ProcessBase
    {
        private ITagParseControl tagParse;

        private bool forceReload = false;

        private IHIRTFile _file;
        public event EventHandler<ITagInstance> OnInstanceLoadEvent;

        public ReadTagInstanceProcessV2(IHIRTFile file)
        {
            _file = file;
        }

        public ITagParseControl TagParse { get => tagParse; set => tagParse = value; }

        protected override async Task OnExecuting()
        {
            if (_file is SSpaceFile)
            {
                SSpaceFile file = (SSpaceFile)_file;
                if (file.IsDeserialized)
                {
                    forceReload = file.Deserialized(_onDeserialized: OnInstanceLoadEvent).TagParse.ParseControlFiltter != null;
                }
                tagParse = file.Deserialized(forceReload: forceReload, _onDeserialized: OnInstanceLoadEvent).TagParse;
            }
            else if (_file is TagStructMemFile)  {
                TagStructMemFile _fileMem = (TagStructMemFile)_file;
                if (HIFileContext.RuntimeTagLoader.checkLoadTagInstance(_fileMem.ObjectId))
                {
                    tagParse = new TagParseControlMem(_fileMem.TagGroup, _fileMem.M);
                    (tagParse as TagParseControlMem).Address = _fileMem.TagData;
                    tagParse.readFile();
                    
                }
            }

        }

    }
}
