using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.UI.Modals;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(ShaderBytecodeFile))]
    public class ShaderBytecodeViewModel : SSpaceFileViewModel<ShaderBytecodeFile>, IDisposeWithView
    {
        public string DecompiledStr { get; set; }
        public string DecompiledStrGLSL { get; set; }
        public ShaderBytecodeViewModel(IServiceProvider serviceProvider, ShaderBytecodeFile file) : base(serviceProvider, file)
        {
        }

        protected override async Task OnInitializing()
        {

            if (File != null)
            {
                if (!File.IsDeserialized)
                {


                    var process = new ReadTagInstanceProcessV2(File);

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
                
                await decompileShaderAsync(File.Deserialized().Root);
                
            }
        }

        async Task decompileShaderAsync(TagInstance ti)
        {
            try
            {

                var temp = ti["shaderBytecodeData"] as TagData;

                MemoryStream stream = new MemoryStream(temp?.ReadBuffer());
                ShaderByteCodeDecompileProcess process = new ShaderByteCodeDecompileProcess(temp?.ReadBuffer());

                var modal = ServiceProvider.GetService<ProgressModal>();
                modal.DataContext = process;

                using (modal)
                {
                    Modals.Add(modal);
                    modal.Show();
                    IsBusy = true;

                    await Task.Factory.StartNew(process.Execute, TaskCreationOptions.LongRunning);
                    await process.CompletionTask;

                    DecompiledStr = process.DecompiledStr;
                    DecompiledStrGLSL = process.DecompiledStrGLSL;
                    if (string.IsNullOrEmpty(DecompiledStrGLSL) && !string.IsNullOrEmpty(process.DecompiledStrGLSL_V))
                        DecompiledStrGLSL = process.DecompiledStrGLSL_V;

                    await modal.Hide();
                    Modals.Remove(modal);
                    IsBusy = false;
                }
                var statusList = process.StatusList;
                if (statusList.HasErrors || statusList.HasWarnings)
                    await ShowStatusListModal(statusList);

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());


            }
        }
    }
}
