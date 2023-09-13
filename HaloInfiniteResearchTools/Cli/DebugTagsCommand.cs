
using HaloInfiniteResearchTools.Processes;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using Microsoft.Extensions.DependencyInjection;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace HaloInfiniteResearchTools.Cli
{
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
            var process = new OpenFilesProcess( EntryPoint.ServiceProvider, deploy_dir.FullName);
            process.Completed += OpenFilesProcess_Completed;
            await process.Execute();
            Console.WriteLine("Tags listed to");
        }

        private void OpenFilesProcess_Completed(object? sender, EventArgs e)
        {
            //var founds = EntryPoint.ServiceProvider.GetRequiredService<IHIFileContext>().GetFiles("."+ _type_tag);
            var founds = EntryPoint.ServiceProvider.GetRequiredService<IHIFileContext>().GetFiles(_type_tag);
            StringBuilder outPutPath = new StringBuilder();
            foreach (SSpaceFile _file  in founds) {
                //outPutPath.AppendLine(file.Path_string);
                //if (_file.Name.Contains("-index-") && _file.FileMemDescriptor.GlobalTagId1 == -1)
                if (_file.Name== "2085921000_2085921000-index-1")
                {
                    if (_file.FileMemDescriptor.Resource_count == 0)
                        CheckFileT0(_file);
                }
            }
            //FileStream fileStream= new FileStream(_outfile.FullName,FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //File.WriteAllText(_outfile.FullName, outPutPath.ToString());

            Console.WriteLine(outPutPath);
            Console.WriteLine("Tags listed to "+ founds.Count().ToString());
            
        }

        private static void CheckFileT0(SSpaceFile _file)
        {
            var stre = _file.GetStream();
            stre.Seek(0, SeekOrigin.Begin);
            byte[] temp = new byte[4];
            stre.Read(temp);
            int count_inf = BitConverter.ToInt32(temp);
            List< Dictionary<string, object> > list = new List< Dictionary<string, object> >();
            for (int i = 0; i < count_inf; i++)
            {
                var item = ReadInfoChunk(stre, _file);
                list.Add(item);
            }
        }

        private static Dictionary<string, object> ReadInfoChunk(HIRTStream stre,SSpaceFile _file)
        {
            Dictionary<string,object> result = new Dictionary<string, object>();
            byte[] temp = new byte[4];
            stre.Read(temp);
            result["globalId"] = BitConverter.ToInt32(temp);
            if ( !(_file.Parent as ModuleFile).FilesGlobalIdLookup.ContainsKey((int)result["globalId"])) { 
                Debug.Assert(false);
            }
            stre.Read(temp);
            result["type"] = BitConverter.ToInt32(temp);
            Debug.Assert((int)result["type"] == 1);
            stre.Read(temp);
            result["GlobalSome"]  = BitConverter.ToInt32(temp);
            stre.Read(temp);
            result["RefNegative"] = BitConverter.ToInt32(temp);
            byte[] tempS = new byte[2];
            stre.Read(tempS);
            result["tempShort"] = BitConverter.ToInt16(tempS);
            stre.Read(temp);
            int cantRef = BitConverter.ToInt32(temp);
            result["cantRef"] = cantRef;
            if (cantRef > 0) {
                List<int> regsId = new List<int>();
                for (int i = 0; i < cantRef; i++)
                {
                    stre.Read(temp);
                    regsId.Add(BitConverter.ToInt32(temp));
                }

                result["Refs"] = regsId;
            }
            stre.Read(temp);
            int padding = BitConverter.ToInt32(temp);
            result["padding"] = padding;
            Debug.Assert(padding == 0);
            if (padding != 0)
            {

            }
            return result;
        }

        private static void CheckFile(SSpaceFile _file)
        {
            var stre = _file.GetStream();
            stre.Seek(0, SeekOrigin.Begin);
            var mod = stre.Length % 4;
            var count = stre.Length / 1;
            Dictionary<int, (int, string)> id_tags = new Dictionary<int, (int, string)>();
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

                if (global_id != -1 && (_file.Parent as ModuleFile).FilesGlobalIdLookup.ContainsKey(global_id) && !id_tags.ContainsKey(global_id))
                {
                    id_tags[global_id] = (i, data_str);
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
                if (count_inf > 5 && count_inf < 20)
                {
                    foreach (var item in id_tags.Keys)
                    {
                        stre.Seek(id_tags[item].Item1, SeekOrigin.Begin);
                        byte[] temp = new byte[4];
                        stre.Read(temp);
                        int val_1 = BitConverter.ToInt32(temp);
                        ISSpaceFile temp_f = (_file.Parent as ModuleFile).FilesGlobalIdLookup[val_1];
                        stre.Read(temp);
                        int val_2 = BitConverter.ToInt16(temp, 0);
                        int val_2_0 = BitConverter.ToInt16(temp, 2);
                        Debug.Assert(val_2 == 1);
                        stre.Read(temp);
                        int val_3 = BitConverter.ToInt32(temp);
                        stre.Read(temp);
                        int val_4 = BitConverter.ToInt32(temp);
                        Debug.Assert(val_4 == -1);
                        stre.Read(temp);
                        int val_5 = BitConverter.ToInt32(temp);
                        stre.Read(temp);
                        int val_6 = BitConverter.ToInt32(temp);
                    }
                }
            }
            else
            {
                Debug.Assert(id_tags.Keys.Count == c_ch - 1);
            }
        }
    }
}
