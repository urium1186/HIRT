using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.UI.Modals;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(ShaderBytecodeFile))]
    public class ShaderBytecodeViewModel : SSpaceFileViewModel<ShaderBytecodeFile>, IDisposeWithView
    {
        public string DecompiledStr { get; set; }
        public ShaderBytecodeViewModel(IServiceProvider serviceProvider, ShaderBytecodeFile file) : base(serviceProvider, file)
        {
        }

        protected override async Task OnInitializing()
        {

            if (File != null)
            {
                var process = new ReadTagInstanceProcess(File);

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
            }
        }

        private void TagParse_OnInstanceLoadEvent(object? sender, ITagInstance e)
        {

            if (e != null)
            {
                string classHash = ((TagInstance)e).TagDef.E?["hash"].ToString();
                switch (classHash)
                {
                    case "B02683786045FFD03AE948A2C2F397C4":
                        decompileShaderAsync((TagInstance)e);
                        break;
                    default:
                        break;
                }
            }
        }

        async Task decompileShaderAsync(TagInstance ti)
        {
            try
            {
                var temp = ti["shaderBytecodeData"] as FUNCTION;

                MemoryStream stream = new MemoryStream(temp?.ReadBuffer());
                ShaderByteCodeDecompileProcess process = new ShaderByteCodeDecompileProcess(temp?.ReadBuffer());

                await Task.Factory.StartNew(process.Execute, TaskCreationOptions.LongRunning);
                await process.CompletionTask;

                DecompiledStr = process.DecompiledStr;

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());


            }
        }
    }
}
