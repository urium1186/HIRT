using LibHIRT.ModuleUnpacker;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    internal class LoadFilePathFromTxProcess : ProcessBase
    {
        private string pathTextFilesPath;
        private List<Dictionary<string, object>> _inDiskPathDB;

        public LoadFilePathFromTxProcess(string pathTextFilesPath)
        {
            this.pathTextFilesPath = pathTextFilesPath;
        }

        protected override async Task OnExecuting()
        {
            
            FileInDiskPathDA.GetInDiskPath(out _inDiskPathDB);
            List<Dictionary<string, object>> pathErrors = findAllWitErrors();

            StreamReader sr = new StreamReader(pathTextFilesPath);
            while (!sr.EndOfStream)
            {
                string tempLine = sr.ReadLine().Replace("\0", "");
                if (string.IsNullOrEmpty(tempLine))
                    continue;
                var splits = tempLine.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                int idToFound = Mmr3HashLTU.fromStrHash(splits[0]);
                if (idToFound == -1)
                    continue;
                List<Dictionary<string, object>> salida = null;// getInFilePathOnDb(idToFound);
                if (salida != null)
                {
                    if (salida.Count != 0)
                    {
                        if (salida.Count > 1)
                        {
                        }
                        else {
                            if (salida[0]["path_string"].ToString() != splits[1])
                            {

                            }
                            else { 
                            }
                        }
                    }
                    else { 
                    }
                }
                else { 
                
                }
            }
        }

        List<Dictionary<string, object>> getInFilePathOnDb(int fileId)
        {
            if (_inDiskPathDB == null)
                return null;
            List<Dictionary<string, object>> result = _inDiskPathDB.FindAll(c => (int)c["file_id"] == fileId);
            foreach (var item in result)
            {
                _inDiskPathDB.Remove(item);
            }
            return result;
        }

        List<Dictionary<string, object>> findAllWitErrors()
        {
            if (_inDiskPathDB == null)
                return null;
            List<Dictionary<string, object>> result = _inDiskPathDB.FindAll(c => !c["path_string"].ToString().Contains('.'));
            foreach (var item in result)
            {
                _inDiskPathDB.Remove(item);
            }
            return result;
        }

    }
}
