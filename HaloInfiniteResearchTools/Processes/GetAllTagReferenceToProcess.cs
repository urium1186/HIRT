using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    class GetAllTagReferenceToProcess : ProcessBase<List<EntryRef>>
    {
        int globalId = -1;
        List<EntryRef> entryRefs= new List<EntryRef>(); 

        public GetAllTagReferenceToProcess(int globalId)
        {
            this.globalId = globalId;
        }

        public override List<EntryRef> Result => entryRefs;

        protected override async Task OnExecuting()
        {
            entryRefs = HIFileContext.Instance.getAllTagReferenceTo(globalId);
        }
    }
}
