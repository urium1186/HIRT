using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.Files.Base;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using HaloInfiniteResearchTools.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(PsoDictionaryFile))]
    public class PsoDictionaryViewModel : SSpaceFileViewModel<PsoDictionaryFile>, IDisposeWithView
    {
        private readonly ITabService _tabService;

        public ICommand OpenGenFileViewCommand { get; }
        public PsoDictionaryViewModel(IServiceProvider serviceProvider, PsoDictionaryFile file) : base(serviceProvider, file)
        {
            OpenGenFileViewCommand= new AsyncCommand<int>(OpenGenFileTab);
            _tabService = serviceProvider.GetService<ITabService>();
        }

        private async Task OpenGenFileTab(int arg)
        {
            IHIRTFile file = HIFileContext.Instance.GetFile(arg);
            if (file == null)
            {
                await ShowMessageModal(
                            title: "Ref to file not found.",
                            message: $"We can't open the global id {arg}.");
                return;
            }



            if (!_tabService.CreateTabForFile(file, out _, true))
            {

                await ShowMessageModal(
                  title: "Unsupported File Type",
                  message: $"We can't open {file.TagGroup} files yet.");

                return;
            }
        }

        protected override Task OnInitializing()
        {
            if (File!= null)
            {
                File.ReadFile();
            }
            return base.OnInitializing();
        }
    }
}
