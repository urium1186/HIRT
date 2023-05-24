using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using HelixToolkit.SharpDX.Core.Assimp;
using LibHIRT.Files.FileTypes;
using System;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(LevelFile))]
    public class LevelViewModel : SSpaceFileViewModel<LevelFile>, IDisposeWithView
    {
        public LevelViewModel(IServiceProvider serviceProvider, LevelFile file) : base(serviceProvider, file)
        {

        }

        private async Task PrepareModelViewer(Assimp.Scene assimpScene)
        {
            //await ReadMeshAttachmentsModelsAsync(assimpScene);
            var importer = new Importer();
            importer.ToHelixToolkitScene(assimpScene, out var scene);

        }

        protected override async Task OnInitializing()
        {

            ReadTagInstanceProcess readTag = new ReadTagInstanceProcess(File);
            readTag.OnInstanceLoadEvent += ReadTag_OnInstanceLoadEvent;
            await RunProcess(readTag);
            using (var prog = ShowProgress())
            {
                prog.Status = "Preparing Viewer";
                prog.IsIndeterminate = true;

                //await PrepareModelViewer(convertProcess.Result);
            };
        }

        private void ReadTag_OnInstanceLoadEvent(object? sender, LibHIRT.TagReader.ITagInstance e)
        {
            if (e is LibHIRT.TagReader.FlagGroup && e.TagDef.xmlPath.Item2.Contains("transform flags"))
            {
                var tra_fl = e as LibHIRT.TagReader.FlagGroup;
                if (tra_fl.Options_v[0])
                {
                }
            }
        }
    }
}
