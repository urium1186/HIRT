using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.Services;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfHexaEditor.Core.MethodExtention;

namespace HaloInfiniteResearchTools.ViewModels
{
    public class ToolsViewModel : ViewModel
    {
        string _str_value="";
        int _int_value=-1;
        string _str_hash = "";
        bool _forceLowerCase = true;
        private IHIFileContext _hiFileContext;
        string _textFilesPath = "";
        string _searchTerm = "";

        public ToolsViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _hiFileContext = ((App)App.Current).ServiceProvider.GetRequiredService<IHIFileContext>();
        }

        public string Str_value { get => _str_value; set { 
                _str_value = value;
                if (_forceLowerCase && _str_value.ToLower() != value) { 
                    Str_value = value.ToLower();
                }
                
            }  
        }
        public int Int_value { get => _int_value; set { 
                _int_value = value; 
            } }
        public string Str_hash { get => _str_hash; set => _str_hash = value; }
        public bool ForceLowerCase { get => _forceLowerCase; set => _forceLowerCase = value; }
        public string TextFilesPath { get => _textFilesPath; set => _textFilesPath = value; }
        public string SearchTerm { get => _searchTerm; set => _searchTerm = value; }

        public void GenerateFromStrValue() {
            Str_hash = Mmr3HashLTU.getMmr3HashFrom(_str_value);
            Int_value = Mmr3HashLTU.getMmr3HashIntFrom(_str_value);
            
        }

        public bool CheckInUse()
        {
            return Mmr3HashLTU.Mmr3lTU.ContainsKey(Mmr3HashLTU.getMmr3HashIntFrom(_str_value));
        }

        public bool AddUniqueStrValue()
        {
            return Mmr3HashLTU.AddUniqueStrValue(_str_value);
        }

        public async void  ProcessAllMmh3AllBytecodeToTxt()
        {
            if (_hiFileContext == null)
                return;
            var files = _hiFileContext.GetFiles<ShaderBytecodeFile>();
            var prefService = ServiceProvider.GetRequiredService<IPreferencesService>();
             string pathExport = prefService.Preferences.DefaultExportPath+ "\\shader_str\\";
            Directory.CreateDirectory(pathExport);
            foreach ( var file in files )
            {
                if (File.Exists(pathExport + file.FileMemDescriptor.GlobalTagId1 + ".txt"))
                    continue;
                ShaderBytecodeViewModel temp = new ShaderBytecodeViewModel(ServiceProvider, file);
                temp.PropertyChanged += Temp_PropertyChangedAsync;
                await temp.Initialize();
                
            }
        }
        public async void  ProcessAllBytecodeToTxtInShaderVariant(string searchPattern)
        {
            if (_hiFileContext == null)
                return;
            var files = _hiFileContext.GetFiles<ShaderVariantFile>(searchPattern);
            var count = files.Count();
            var prefService = ServiceProvider.GetRequiredService<IPreferencesService>();
             string pathExport = prefService.Preferences.DefaultExportPath+ "\\shader_str\\";
            Directory.CreateDirectory(pathExport);
            foreach ( var file in files )
            {
                var sh_bc = getAllShaderByteCodeIn(file);
                foreach (var sh in sh_bc) {
                    if (File.Exists(pathExport + sh.FileMemDescriptor.GlobalTagId1 + ".txt"))
                        continue;
                    ShaderBytecodeViewModel temp = new ShaderBytecodeViewModel(ServiceProvider, sh);
                    temp.PropertyChanged += Temp_PropertyChangedAsync;
                    await temp.Initialize();
               
                }



            }
        }


        private List<ShaderBytecodeFile> getAllShaderByteCodeIn(ShaderVariantFile file) {
            var root = file.Deserialized?.Root;
            List < ShaderBytecodeFile > result = new List<ShaderBytecodeFile> ();
            
            if (root == null) 
                return result;
            ListTagInstance shader_groups = root["shader groups"] as ListTagInstance;
            if (shader_groups == null)
                return result;
            Dictionary<int, ShaderBytecodeFile> dicShaderBC = new Dictionary<int, ShaderBytecodeFile>();
            foreach (var shader_group in shader_groups) {
                ListTagInstance shaders = shader_group["shaders"] as ListTagInstance;
                if (shaders == null)
                    continue;
                foreach (var sh in shaders)
                {
                    TagRef tagRef = sh["shader bytecode"] as TagRef;
                    if (tagRef == null) continue;
                    if (dicShaderBC.ContainsKey(tagRef.Ref_id_int)) continue;
                    var filesBC = _hiFileContext.GetFiles<ShaderBytecodeFile>(tagRef.Ref_id);
                    if (filesBC.Count() != 1)
                        continue;
                    foreach (var item in filesBC)
                    {
                        dicShaderBC[tagRef.Ref_id_int] = item;
                    }

                }
            }

            if (dicShaderBC.Count > 0)
                return dicShaderBC.Values.ToList();
            return result;
        }



        private void Temp_PropertyChangedAsync(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DecompiledStr" && (sender is ShaderBytecodeViewModel) )
            {
                var prefService = ServiceProvider.GetRequiredService<IPreferencesService>();
                string pathExport = prefService.Preferences.DefaultExportPath + "\\shader_str\\";
                string tempString = (sender as ShaderBytecodeViewModel).DecompiledStr;
                if (string.IsNullOrEmpty(tempString))
                    return;
                string tempFilename = pathExport + (sender as ShaderBytecodeViewModel).File.FileMemDescriptor.GlobalTagId1 + ".txt";
                FileStream tempStream = new FileStream(tempFilename, FileMode.CreateNew, FileAccess.Write);

                foreach (var item in tempString.ToArray())
                {
                    tempStream.Write(BitConverter.GetBytes(item));
                }

                tempStream.FlushAsync();
                tempStream.Close();
            }
        }

        public async void doSome() {
            if (Directory.Exists(TextFilesPath))
            {

                List<string>? list = new List<string>();
                list.Add(TextFilesPath);
                var process = new TextToMmh3LTUProcess(list);
                process.Completed += ProcessTextToMmh3_Completed;
                await RunProcess(process);
            }
        }

        private void ProcessTextToMmh3_Completed(object? sender, EventArgs e)
        {
            if (sender is TextToMmh3LTUProcess) {
                if ((sender as TextToMmh3LTUProcess).DbModify) 
                    MessageBox.Show("Saved to DB");
                else
                {
                    MessageBox.Show("Not need to save.");
                }
            } 
        }
    }
}
