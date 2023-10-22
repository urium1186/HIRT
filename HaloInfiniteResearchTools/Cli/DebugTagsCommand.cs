
using HaloInfiniteResearchTools.Processes;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace HaloInfiniteResearchTools.Cli
{
    public class ModeuleRefMap {
        SSpaceFile _file;
        int _globalId = -1;
        int _cant1 = -1;
        int _extarnal = -1;
        int _negative = -2;
        short _temp = -1;
        int _cantRef = -1;
        List<(int, SSpaceFile)> _refFiles = new List<(int, SSpaceFile)>();

        public ModeuleRefMap()
        {
        }

        public SSpaceFile File { get => _file; set => _file = value; }
        public int GlobalId { get => _globalId; set => _globalId = value; }
        public int Cant1 { get => _cant1; set => _cant1 = value; }
        public int Extarnal { get => _extarnal; set => _extarnal = value; }
        public int Negative { get => _negative; set => _negative = value; }
        public short Temp { get => _temp; set => _temp = value; }
        public int CantRef { get => _cantRef; set => _cantRef = value; }
        public List<(int, SSpaceFile)> RefFiles { get => _refFiles; set => _refFiles = value; }
    }
    public class DebugTagCommand : Command
    {
        private string _type_tag;
        private string _infile;
        private FileInfo? _outfile;

        public DebugTagCommand() : base("debug", "JUST FOR DEBUG")
        {
            AddCommand(
                new Command("index", "DO NOT USE")
                    {
                        new Option<DirectoryInfo>(new string[] { "--deploy", "-d" }, "Deploy dir"),
                        new Option<string>(new string[] { "--type", "-t" }, "Output file name"),
                        new Option<FileInfo?>(new string[] { "--output", "-o" }, "Output file name"),
                    }
                .Also(cmd => cmd.SetHandler(
                    (DirectoryInfo deploy_dir, string infile, FileInfo? outfile, InvocationContext ctx) => ListTagOsHandler(deploy_dir, infile, outfile, true, ctx),
                    cmd.Options[0],
                    cmd.Options[1],
                    cmd.Options[2])));
            /*
            AddCommand(
                new Command("unpack", "Unpack a bond file")
                    {
                        new Argument<FileInfo>("filename", "Bond file to unpack"),
                        new Option<FileInfo?>(new string[] { "--output", "-o" }, "Output file name"),
                    }
                .Also(cmd => cmd.SetHandler(
                    (FileInfo infile, FileInfo? outfile, InvocationContext ctx) => CacheFileUnpackHandler(infile, outfile, true, ctx),
                    cmd.Arguments[0],
                    cmd.Options[0])));
            AddCommand(
               new Command("pack", "Pack a bond file")
                    {
                        new Argument<FileInfo>("filename", "Bond file to pack"),
                        new Option<FileInfo?>(new string[] { "--output", "-o" }, "Output file name")
                    }
                    .Also(cmd => cmd.SetHandler(
                        (FileInfo infile, FileInfo? outfile, InvocationContext ctx) => CacheFilePackHandler(infile, outfile, ctx),
                        cmd.Arguments[0],
                        cmd.Options[0])));
            */

        }

        private async void ListTagOsHandler(DirectoryInfo deploy_dir, string type_tag, FileInfo? outfile, bool v, InvocationContext ctx)
        {
            _type_tag = type_tag;
            _outfile = outfile;
            var process = new OpenFilesProcess(EntryPoint.ServiceProvider, deploy_dir.FullName);
            process.Completed += OpenFilesProcess_Completed;
            await process.Execute();
            Console.WriteLine("Tags listed to");
        }

        private void OpenFilesProcess_Completed(object? sender, EventArgs e)
        {
            try
            {
                //var founds = EntryPoint.ServiceProvider.GetRequiredService<IHIFileContext>().GetFiles("."+ _type_tag);
                var founds = EntryPoint.ServiceProvider.GetRequiredService<IHIFileContext>().GetFiles(_type_tag);
                StringBuilder outPutPath = new StringBuilder();
                foreach (SSpaceFile _file in founds)
                {
                    //outPutPath.AppendLine(file.Path_string);
                    //if (_file.Name.Contains("-index-") && _file.FileMemDescriptor.GlobalTagId1 == -1)
                    //if (_file.Name == "2085921000_2085921000-index-1")
                    if (_file.Name.Contains("-index-1"))
                    {
                        Dictionary<int, List<int>> id_tags = null;
                        List<(int, int, int, SSpaceFile)> id_addres_tags = null;
                        if (_file.FileMemDescriptor.Resource_count == 0) {
                            CheckFile(_file, out id_tags, out id_addres_tags);
                            CheckFileT0(_file, id_tags, id_addres_tags);

                        }
                            

                    }
                }
                //FileStream fileStream= new FileStream(_outfile.FullName,FileMode.OpenOrCreate, FileAccess.ReadWrite);
                //File.WriteAllText(_outfile.FullName, outPutPath.ToString());

                Console.WriteLine(outPutPath);
                Console.WriteLine("Tags listed to " + founds.Count().ToString());
            }
            catch (Exception ex)
            {

                throw ex;
            }
            

        }
        private static void CheckFileT0(SSpaceFile _file, Dictionary<int, List<int>> id_tags, List<(int, int, int, SSpaceFile)> id_addres_tags) {
            var stre = _file.GetStream();
            stre.Seek(0, SeekOrigin.Begin);
            byte[] temp = new byte[4];
            stre.Read(temp);
            int count_inf = BitConverter.ToInt32(temp);

            Debug.Assert(count_inf == id_tags.Count);
            for (int i = 0; i < id_tags.Values.Count; i++)
            {
                int len = 0;
                if (i < id_tags.Count - 1) {
                    len = id_tags.Values.ElementAt(i + 1)[0] - id_tags.Values.ElementAt(i)[0];
                }
                byte[] array = new byte[len];
            }
        }
        private static void CheckFileT0(SSpaceFile _file)
        {
            var stre = _file.GetStream();
            stre.Seek(0, SeekOrigin.Begin);
            byte[] temp = new byte[4];
            stre.Read(temp);
            int count_inf = BitConverter.ToInt32(temp);
            int modules_ref = (_file.Parent as ModuleFile).Children.Count();
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            for (int i = 0; i < count_inf; i++)
            {
                var item = ReadInfoChunk(stre, _file);
                list.Add(item);
            }
        }

        private static Dictionary<string, object> ReadInfoChunk(HIRTStream stre, SSpaceFile _file)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            byte[] temp = new byte[4];
            stre.Read(temp);
            result["globalId"] = BitConverter.ToInt32(temp);
            SSpaceFile file_ref = null;
            if (!(_file.Parent as ModuleFile).FilesGlobalIdLookup.ContainsKey((int)result["globalId"]))
            {
                Debug.Assert(false);
            }
            else {
                file_ref = (SSpaceFile?)(_file.Parent as ModuleFile).FilesGlobalIdLookup[((int)result["globalId"])];
            }
            result["file_ref"] = file_ref;
           stre.Read(temp);
            result["cant"] = BitConverter.ToInt32(temp);
            Debug.Assert((int)result["cant"] == 1);
            
            for (int j = 0; j < (int)result["cant"]; j++)
            {
                stre.Read(temp);
                result["GlobalSome"] = BitConverter.ToInt32(temp);
                stre.Read(temp);
                result["RefNegative"] = BitConverter.ToInt32(temp);
                byte[] tempS = new byte[2];
                stre.Read(tempS);
                result["tempShort"] = BitConverter.ToInt16(tempS);
                stre.Read(temp);
                int cantRef = BitConverter.ToInt32(temp);
                result["cantRef"] = cantRef;
                if (cantRef > 0)
                {
                    List<ISSpaceFile> regsId = new List<ISSpaceFile>();
                    for (int i = 0; i < cantRef; i++)
                    {
                        stre.Read(temp);
                        int sub_id = BitConverter.ToInt32(temp);
                        //regsId.Add(BitConverter.ToInt32(temp));
                        if ((_file.Parent as ModuleFile).FilesGlobalIdLookup.ContainsKey(sub_id))
                        {
                            var subFile = (SSpaceFile?)(_file.Parent as ModuleFile).FilesGlobalIdLookup[sub_id];
                            regsId.Add(subFile);
                        }
                        else
                        {

                        }

                    }

                    result["Refs"] = regsId;
                    
                }
                stre.Read(temp);
                int padding = BitConverter.ToInt32(temp);
                result["padding"] = padding;
                Debug.Assert(padding == 0);
                if (padding != 0)
                {
                    for (int k = 0; k < padding; k++)
                    {
                        stre.Read(temp);
                        int subGlobal = BitConverter.ToInt32(temp);
                        if ((_file.Parent as ModuleFile).FilesGlobalIdLookup.ContainsKey(subGlobal))
                        {
                            var subFile = (SSpaceFile?)(_file.Parent as ModuleFile).FilesGlobalIdLookup[subGlobal];
                            //regsId.Add(subFile);
                        }
                        else
                        {

                        }
                    }
                }
                else {
                   
                }

            }
            
            return result;
        }

        private static void CheckFile(SSpaceFile _file, out Dictionary<int, List<int>> id_tags, out List<(int, int, int, SSpaceFile)> id_addres_tags)
        {
            var stre = _file.GetStream();
            stre.Seek(0, SeekOrigin.Begin);
            var mod = stre.Length % 4;
            var count = stre.Length / 1;
            
            id_tags = new Dictionary<int, List<int>>();
            id_addres_tags = new List<(int,int, int, SSpaceFile)>();
            int c_ch = (_file.Parent as ModuleFile).FilesGlobalIdLookup.Count();
            int count_inf = -1;
            string data_str = "";
            for (int i = 0; i < count - 4; i++)
            {

                byte[] temp = new byte[4];
                stre.Seek(i, SeekOrigin.Begin);
                stre.Read(temp);

                int global_id = BitConverter.ToInt32(temp);
                if (i == 0)
                {
                    count_inf = global_id;
                    long t_0 = (stre.Length - 4) / count_inf;
                    long t_1 = (stre.Length - 4) % count_inf;
                }

                if (global_id != -1)
                {
                    if ((_file.Parent as ModuleFile).FilesGlobalIdLookup.ContainsKey(global_id))
                    {
                        if (!id_tags.ContainsKey(global_id))
                        {
                            id_tags[global_id] = new List<int>();
                            id_tags[global_id].Add(i);
                        }
                        else
                        {
                            id_tags[global_id].Add(i);
                        }
                        id_addres_tags.Add((i, global_id, id_addres_tags.Count == 0 ? 4 : i - id_addres_tags[id_addres_tags.Count - 1].Item1, (SSpaceFile)(_file.Parent as ModuleFile).FilesGlobalIdLookup[global_id]));
                    }
                    else
                    {
                        int dife = id_addres_tags.Count == 0 ? 4 : i - id_addres_tags[id_addres_tags.Count - 1].Item1;
                        if ((global_id >-1 && global_id < 10) && dife >= 4)
                        {

                            id_addres_tags.Add((i, global_id, dife, null));
                        }

                    }
                }
                else {
                    int dife = id_addres_tags.Count == 0 ? 4 : i - id_addres_tags[id_addres_tags.Count - 1].Item1;
                    if (dife >= 4)
                    {

                        id_addres_tags.Add((i, global_id, dife, null));
                    }
                }
            }
            /*if (id_tags.Keys.Count != 0)
                Debug.Assert((id_tags[id_tags.Keys.ElementAt(0)] == 4 && _file.FileMemDescriptor.Resource_count == 0 && c_ch  == count_inf) || _file.FileMemDescriptor.Resource_count == 2);
            else { 
            }*/

            if (_file.FileMemDescriptor.Resource_count == 0)
            {
                Debug.Assert(id_tags.Keys.Count == c_ch - 1);
                Debug.Assert(id_tags.Keys.Count == count_inf);
            }
            else
            {
                Debug.Assert(id_tags.Keys.Count == c_ch - 1);
            }
        }
    }
}
