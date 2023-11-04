using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    public class TagStructsLoadAllProcess : ProcessBase<IEnumerable<string>>
    {
        private List<string> tags;

        public override IEnumerable<string> Result => tags;

        protected override async Task OnExecuting()
        {
            string tempDirPath = Directory.GetCurrentDirectory() + "\\TagReader\\Tags\\";
            if (Directory.Exists(TagXmlParse.TagsPath)) {
                tempDirPath = TagXmlParse.TagsPath + "\\";
                TagXmlParseV2.TagsPath = TagXmlParse.TagsPath;

            }
                
            DirectoryInfo d = new DirectoryInfo(tempDirPath);
             tags = new List<string>();
            FileInfo[] _filePaths = d.GetFiles("*.xml");
            Status = _filePaths.Length > 1 ? "Opening Files" : "Opening File";
            UnitName = _filePaths.Length > 1 ? "files opened" : "file opened";
            TotalUnits = _filePaths.Length;
            IsIndeterminate = _filePaths.Length == 1;
            
            var objLock = new object();
            Parallel.ForEach(_filePaths, file =>
            {
                string fileName = file.Name;
                Status = file.Name;
                try
                {
                    var _tagLayout = TagXmlParseV2.parse_the_mfing_xmls(file.Name.Replace(".xml", ""));
                    if (_tagLayout.Count != 0)
                    {
                        tags.Add(file.Name);
                        StatusList.AddMessage(fileName, "Correct tag template.");
                    }
                    else {
                        StatusList.AddWarning(fileName, "No correct tag template.");
                    }
                        
                }
                catch (Exception ex)
                {
                    StatusList.AddError(fileName, ex);
                }
                finally
                {
                    lock (objLock)
                    {
                        CompletedUnits++;
                    }

                }
            });
        }
    }
}
