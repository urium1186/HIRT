using Aspose.ThreeD.Utilities;
using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.UI.Modals;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using ImageMagick;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using Microsoft.Extensions.DependencyInjection;
using OpenSpartan.Grunt.Models.HaloInfinite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vim.Math3d;

namespace HaloInfiniteResearchTools.ViewModels
{
    [AcceptsFileType(typeof(ShaderBytecodeFile))]
    public class ShaderBytecodeViewModel : SSpaceFileViewModel<ShaderBytecodeFile>, IDisposeWithView
    {
        public string DecompiledStr { get; set; }
        public string DecompiledStrGLSL { get; set; }
        public string DecompiledStrHLSL { get; set; }
        public string ModDecompiledStrHLSL { get; set; }
        public string DecompiledStrSPV { get; set; }
        public string DecompiledDxil { get; set; }
        public bool IsPixelShader { get; private set; }
        public bool IsVertexShader { get; private set; }

        public ShaderBytecodeViewModel(IServiceProvider serviceProvider, ShaderBytecodeFile file) : base(serviceProvider, file)
        {
            ModDecompiledStrHLSL = "";
        }

        protected override async Task OnInitializing()
        {
            IsPixelShader = false;
            IsVertexShader = true;

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
                    //if (string.IsNullOrEmpty(DecompiledStrGLSL) && !string.IsNullOrEmpty(process.DecompiledStrGLSL_V))
                    DecompiledStrHLSL = process.DecompiledStrHLSL;
                    ModDecompiledStrHLSL = DecompiledStrHLSL;
                    DecompiledStrSPV = process.DecompiledStrSPV;
                    DecompiledDxil = process.DecompiledDxil;
                    //else {
                    //    DecompiledStrGLSL_V = "";
                    //}
                    createMetadatasFromAsm();
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

                throw e;
            }
        }

        void createMetadatasFromAsm()
        {
            if (string.IsNullOrEmpty(DecompiledStr))
                return;
            if (DecompiledStr.IndexOf("; Pixel Shader") != -1)
            {
                IsPixelShader = true;
            }
            if (DecompiledStr.IndexOf("; Vertex Shader") != -1)
            {
                IsVertexShader = true;
            }
            // ";   } $Globals;  " ";   struct $Globals" 

            int posInitGlb = DecompiledStr.IndexOf("struct $Globals");
            int posLastGlb = DecompiledStr.IndexOf(";   } $Globals;");
            if (posLastGlb != -1)
            {
                posLastGlb = DecompiledStr.IndexOf("\r\n;", posLastGlb, 90);
            }

            string structGlobal = "";
            if (posLastGlb != -1)
            {
                structGlobal = DecompiledStr.Substring(posInitGlb, posLastGlb - posInitGlb).Replace("\r\n;", "\r\n");
            }

            //_Globals_m0[

            string strucS = structGlobal;
            string linefield = @"(\s+(\w+)\s+(\w+);\s+;\s+Offset:\s+(\d+)(\r\n)+)*";
            Regex regexAll = new Regex(@"(\bstruct\s+(\$\w+)\r\n\s+({s*(\r\n)+" + linefield + "))", RegexOptions.IgnoreCase);
            Regex regex = new Regex(@"(?<estructura>\bstruct\s+(?<nombre>\$*\w+)\r\n\s+(?<cuerpo>{s*((\r\n)*.(\r\n)*)*})\s+\k<nombre>;\s*;\s*(Offset:\s+(?<offset>\d+))*(\s+Size:\s+(?<tamano>\d+))*)", RegexOptions.IgnoreCase); // (s*(ws*)+\r\n)+ Offset:\s+(\d+)
            Regex regexField = new Regex(@"(?<fields>\s+(?<tipo>[a-z]+(?<tipoDimesion>\d?))\s+(?<nombre>\w+);\s+;\s+Offset:\s+(?<offset>\d+)(\r\n)+)", RegexOptions.IgnoreCase);

            //(?<offset>\d+)


            Match match = regex.Match(strucS);
            List<Match> matches = new List<Match>();
            List<Match> cbufferNames = new List<Match>();
            if (match.Success)
            {
                string structura = match.Groups["estructura"].Value;
                string nombreStruct = match.Groups["nombre"].Value;
                string cuerpo = match.Groups["cuerpo"].Value;
                int offset = int.Parse(match.Groups["offset"].Value);
                int tamano = int.Parse(match.Groups["tamano"].Value);

                MatchCollection fieldMatchs = regexField.Matches(cuerpo);


                Dictionary<int, Match> fields = findAllSubStruct(cuerpo);

                if (fieldMatchs.Count > 0)
                {
                    foreach (Match item in fieldMatchs)
                    {
                        if (fields.Count > 0)
                        {
                            int currentKeyPos = 0;
                            foreach (var structItem in fields)
                            {
                                if (item.Index > structItem.Key)
                                {
                                    matches.Add(structItem.Value);
                                    currentKeyPos++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            for (int i = 0; i < currentKeyPos; i++)
                            {
                                if (i < fields.Count)
                                {
                                    fields.Remove(fields.Keys.ElementAt(i));
                                }
                            }
                            matches.Add(item);



                        }
                        else
                        {
                            matches.Add(item);
                        }
                    }
                }
                foreach (var structItem in fields)
                {
                    matches.Add(structItem.Value);
                }

                cbufferNames = createArrayBufferNames(matches, tamano);
            }

            if (!string.IsNullOrEmpty(this.DecompiledStrHLSL))
            {
                Regex regexCallToGlobalOnHlsls = new Regex(@"(\b_Globals_m0\[(?<index>\w+)\])", RegexOptions.IgnoreCase);
                Regex regexCallToGlobalOnHlslsCall = new Regex(@"(?<toreplace>\b_Globals_m0\[(?<index>\d+)u\]\.(?<arraypos>[xyzw]))", RegexOptions.IgnoreCase);
                Regex regexCallToGlobalOnHlslsCreate = new Regex(@"(\r\n\s+\w+\s+(?<varname>\w+)\s=\s\w+\(\b_Globals_m0\[(?<index>\d+)u\]\);\r\n\s+\w+\s+(?<toreplace>\w+)\s=\s\k<varname>\.(?<arraypos>[xyzw]);)", RegexOptions.IgnoreCase); // 
                MatchCollection gloablCalls = regexCallToGlobalOnHlsls.Matches(DecompiledStrHLSL);
                MatchCollection gloablCallsCalls = regexCallToGlobalOnHlslsCall.Matches(DecompiledStrHLSL);
                MatchCollection gloablCallsInst = regexCallToGlobalOnHlslsCreate.Matches(DecompiledStrHLSL);
                if (gloablCalls.Count > 1)
                {
                    if (gloablCalls.Count != 1 + gloablCallsCalls.Count + gloablCallsInst.Count)
                    {
                    }
                    int count = int.Parse(gloablCalls[0].Groups["index"].Value);
                    Debug.Assert(count * 4 >= cbufferNames.Count);

                    string mod = modCodeHLSL(cbufferNames, gloablCalls, gloablCallsCalls, gloablCallsInst, ModDecompiledStrHLSL);
                    ModDecompiledStrHLSL = mod; 
                }
            }
        }

        string modCodeHLSL(List<Match> cbufferNames, MatchCollection gloablCalls, MatchCollection gloablCallsCalls, MatchCollection gloablCallsInst, string original)
        {
            string modHLSL = original;
            int index = 0;
            HashSet<string> repleced = new HashSet<string>();
            HashSet<string> repetidas = new HashSet<string>();
            int repeticiones = 0;
           
            foreach (Match item in gloablCallsInst)
            {
                if (!repleced.Contains(item.Value))
                {
                    modHLSL = repleceMatch(cbufferNames, modHLSL, repleced, item,true);
                }
                else
                {
                    repetidas.Add(item.Value);
                    repeticiones++;
                }

            }
            foreach (Match item in gloablCallsCalls)
            {
                if (!repleced.Contains(item.Value))
                {
                    modHLSL = repleceMatch(cbufferNames, modHLSL, repleced, item);
                }
                else
                {
                    repetidas.Add(item.Value);
                    repeticiones++;
                }

            }
            return modHLSL;
        }

        private static string repleceMatch(List<Match> cbufferNames, string modHLSL, HashSet<string> repleced, Match item, bool isVarName = false)
        {
            int offset = (item.Groups["arraypos"].Value == "x" ? 0 : item.Groups["arraypos"].Value == "y" ? 1 : item.Groups["arraypos"].Value == "z" ? 2 : item.Groups["arraypos"].Value == "w" ? 3 : 0);
            int inArrayPos = int.Parse(item.Groups["index"].Value) + offset;
            if (inArrayPos < cbufferNames.Count)
            {
                Match replaceBy = cbufferNames[inArrayPos];
                string replaceTo = replaceBy.Groups["nombre"].Value;
                if (replaceBy.Groups.ContainsKey("tipoDimesion") && !string.IsNullOrEmpty(replaceBy.Groups["tipoDimesion"].Value))
                {
                    int tipoDimesion = int.Parse(replaceBy.Groups["tipoDimesion"].Value);

                    int of_pos = inArrayPos % 4;
                    string char_off = of_pos == 0 ? "x" : of_pos == 1 ? "y" : of_pos == 2 ? "z" : of_pos == 3 ? "w" : "";
                    replaceTo = replaceTo + "." + char_off;


                }

                modHLSL = isVarName ? replaceStric(item.Groups["toreplace"].Value, replaceTo, modHLSL) : modHLSL.Replace(item.Groups["toreplace"].Value, replaceTo);
                repleced.Add(item.Value);
            }
            else
            {
            }

            return modHLSL;
        }

        private static string replaceStric(string value, string to_replace, string str__on) {
            string result = str__on;
            int indexFound = result.IndexOf(value);
            while (indexFound!=-1)
            {
                if (indexFound + value.Length  < result.Length)
                {
                    char nextChar = result[indexFound + value.Length];
                    if (!"0123456789".Contains(nextChar))
                    {
                        result = result.Remove(indexFound, value.Length);
                        result = result.Insert(indexFound, to_replace);
                    }
                    else { 
                    
                    }
                }
                else {
                    result.Remove(indexFound, value.Length);
                    result.Insert(indexFound, to_replace);
                }
                indexFound = result.IndexOf(value, indexFound+ to_replace.Length);
            }
            return result;
        }

        Dictionary<int, Match> findAllSubStruct(string cuerpo)
        {
            if (string.IsNullOrEmpty(cuerpo))
                return null;
            Dictionary<int, Match> result = new Dictionary<int, Match>();
            Regex regexSubStruct = new Regex(@"(?<estructura>\bstruct\s+(?<nombre>\w+\.*\w*)((\r\n|\r|\n)*.(\r\n|\r|\n)*)*(?<subBody>{((\r\n|\r|\n)*.(\r\n|\r|\n)*)*})\s+(?<end_nombre>\w+\.*\w*);\s*;*\s*(Offset:\s+(?<offset>\d+))*(\s+Size:\s+(?<size>\d+))*(\r\n))", RegexOptions.IgnoreCase);
            int index_sub_struct = cuerpo.Length;
            int postFirst = 0;
            while (index_sub_struct != -1)
            {
                index_sub_struct = cuerpo.IndexOf("struct struct.", postFirst);
                if (index_sub_struct != -1)
                {
                    int index_of_k = cuerpo.IndexOf("} k_", index_sub_struct);
                    int extra = index_of_k + 80 >= cuerpo.Length - 1 ? cuerpo.Length - 1 : index_of_k + 80;
                    MatchCollection substructsmatchs = regexSubStruct.Matches(cuerpo.Substring(index_sub_struct, extra - index_sub_struct));
                    if (substructsmatchs.Count > 0)
                    {

                        foreach (Match item in substructsmatchs)
                        {
                            result[index_sub_struct + substructsmatchs[0].Index] = item;
                        }


                        postFirst = index_sub_struct + substructsmatchs[0].Index + substructsmatchs[0].Length;
                    }
                    else
                    {
                        postFirst = index_of_k + 5;
                    }


                }
            }
            return result;
        }

        List<Match> createArrayBufferNames(List<Match> fields, int globalSize)
        {
            List<Match> result = new List<Match>();
            if (fields.Count == 0)
                return result;
            int count = 0;
            int lastOffset = 0;
            Match firstMatch = fields[0];
            fields.RemoveAt(0);
            Match previus = firstMatch;
            foreach (Match field in fields)
            {
                int fieldSize = int.Parse(field.Groups["offset"].Value) - int.Parse(previus.Groups["offset"].Value);
                if (fieldSize < 4)
                {

                }
                else
                {
                    int resto = fieldSize % 4;
                    if (resto == 0)
                    {
                        for (int i = 0; i < fieldSize / 4; i++)
                        {
                            result.Add(previus);
                        }
                    }
                    else
                    {

                    }
                }

                /*
                if (field.Groups.ContainsKey("estructura"))
                {

                }
                else {
                    
                    
                    if (fieldSize > 4)
                    {
                        if (previus.Groups.ContainsKey("tipoDimesion") && !string.IsNullOrEmpty(previus.Groups["tipoDimesion"].Value))
                        {
                            int newFiledSize = fieldSize / int.Parse(previus.Groups["tipoDimesion"].Value);
                            Debug.Assert(newFiledSize == 4);
                        }
                        else
                        {

                        }

                    }
                    else if (fieldSize < 4)
                    {


                    }
                    else if (fieldSize == 0)
                    {


                    }
                    else { 
                    
                    }




                }*/
                previus = field;
            }
            if (fields.Count > 0)
            {
                int fieldSize = globalSize - int.Parse(previus.Groups["offset"].Value);
                if (fieldSize < 4)
                {

                }
                else
                {
                    int resto = fieldSize % 4;
                    if (resto == 0)
                    {
                        for (int i = 0; i < fieldSize / 4; i++)
                        {
                            result.Add(previus);
                        }
                    }
                    else
                    {

                    }
                }
            }
            fields.Insert(0, firstMatch);
            return result;
        }
    }
}
