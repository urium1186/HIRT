using HaloInfiniteResearchTools.Common;
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
using System.Windows.Documents;
using System.Windows.Forms.Design;
using System.Windows.Input;
using WpfHexaEditor.Core.MethodExtention;

namespace HaloInfiniteResearchTools.ViewModels
{
    public class ToolsViewModel : ViewModel
    {
        string _str_value = "";
        int _int_value = -1;
        string _str_hash = "";
        bool _forceLowerCase = true;

        private IHIFileContext _hiFileContext;
        string _textFilesPath = "";
        string _searchTerm = "";
        int _vertType = -1;
        bool _onlyVertexShaders = true;
        string _spliters = "";
        private string _pathExport;
        private ConnectXboxServicesResult connectXSR;

        public ICommand ProcessTextCommand { get; }
        public ICommand BCInSVToTxtCommand { get; }
        public ICommand ProcessAllBytecodeToTxtCommand { get; }
        public ICommand GenerateFromStrValueCommand { get; }
        public ICommand CheckInUseCommand { get; }
        public ICommand ProcessLoginCommand { get; }
        public ICommand WebApiCommand { get; }






        public ToolsViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _hiFileContext = ((App)App.Current).ServiceProvider.GetRequiredService<IHIFileContext>();
            ProcessTextCommand = new AsyncCommand(doSome);
            BCInSVToTxtCommand = new AsyncCommand(ProcessAllBytecodeToTxtInShaderVariantMain);
            ProcessAllBytecodeToTxtCommand = new AsyncCommand(ProcessAllBytecodeToTx);
            GenerateFromStrValueCommand = new Command(GenerateFromStrValue);
            CheckInUseCommand = new Command(CheckInUseAction);
            ProcessLoginCommand = new Command(ProcessLogin);
            WebApiCommand = new Command(CallApi);
        }

        private async void ProcessLogin()
        {
            using (var progress = ShowProgress())
            {
                progress.IsIndeterminate = true;
                progress.Status = "Login";
                
                var objLock = new object();
                
                    
                    try
                    {
                    var process = new ConnectXboxServicesProcess();
                    process.Completed += ConnectXboxServicesProcess_Completed;
                    await RunProcess(process);

                }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        lock (objLock)
                        {
                            progress.CompletedUnits++;
                        }

                    }
                


            }

        }

        private async void ConnectXboxServicesProcess_Completed(object? sender, EventArgs e)
        {
            connectXSR = (sender as ConnectXboxServicesProcess).Result;

        }

        private async void CallApi()
        {
            //throw new NotImplementedException();
            using (var progress = ShowProgress())
            {
                progress.IsIndeterminate = true;
                progress.Status = "Login";

                var objLock = new object();


                try
                {
                    var process = new GetFromXboxWebApiProcess(connectXSR);
                    process.Completed += GetFromXboxWebApiProcess_Completed;
                    await RunProcess(process);

                }
                catch (Exception ex)
                {

                }
                finally
                {
                    lock (objLock)
                    {
                      //progress.CompletedUnits++;
                    }

                }



            }
        }

        private void GetFromXboxWebApiProcess_Completed(object? sender, EventArgs e)
        {
            
        }

        private void CheckInUseAction()
        {

        }

        private async Task ProcessAllBytecodeToTx()
        {
            if (_hiFileContext == null)
                return;
            var files = _hiFileContext.GetFiles<ShaderBytecodeFile>();
            var count = files.Count();
            if (count == 0) return;

            var prefService = ServiceProvider.GetRequiredService<IPreferencesService>();
            _pathExport = prefService.Preferences.DefaultExportPath + "\\shader_str\\";
            Directory.CreateDirectory(_pathExport);
            using (var progress = ShowProgress())
            {
                progress.IsIndeterminate = true;
                progress.Status = "Export ByteCodes";

                progress.UnitName = files.Count() > 1 ? "files opened" : "file opened";
                progress.TotalUnits = files.Count();
                progress.IsIndeterminate = files.Count() == 1;

                var objLock = new object();
                foreach (var file in files)
                {
                    string fileName = Mmr3HashLTU.getMmr3HashFromInt(file.FileMemDescriptor.GlobalTagId1);
                    try
                    {

                        if (!File.Exists(_pathExport + Mmr3HashLTU.getMmr3HashFromInt(file.FileMemDescriptor.GlobalTagId1) + ".txt"))
                        {
                            ShaderBytecodeViewModel temp = new ShaderBytecodeViewModel(ServiceProvider, file);
                            temp.PropertyChanged += Temp_PropertyChangedAsync;
                            await temp.Initialize();
                        }



                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        lock (objLock)
                        {
                            progress.CompletedUnits++;
                        }

                    }
                }


            }

        }

        private async Task ProcessAllBytecodeToTxtInShaderVariantMain()
        {
            await ProcessAllBytecodeToTxtInShaderVariant(SearchTerm);
        }

        public string Str_value
        {
            get => _str_value; set
            {
                _str_value = value;
                if (_forceLowerCase && _str_value.ToLower() != value)
                {
                    Str_value = value.ToLower();
                }

            }
        }
        public int Int_value
        {
            get => _int_value; set
            {
                _int_value = value;
            }
        }
        public string Str_hash { get => _str_hash; set => _str_hash = value; }
        public bool ForceLowerCase { get => _forceLowerCase; set => _forceLowerCase = value; }
        public string TextFilesPath { get => _textFilesPath; set => _textFilesPath = value; }
        public string SearchTerm { get => _searchTerm; set => _searchTerm = value; }
        public string Spliters { get => _spliters; set => _spliters = value; }
        public int VertType { get => _vertType; set => _vertType = value; }
        public bool OnlyVertexShaders { get => _onlyVertexShaders; set => _onlyVertexShaders = value; }

        public void GenerateFromStrValue()
        {
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

        public async void ProcessAllMmh3AllBytecodeToTxt()
        {

        }
        public async Task ProcessAllBytecodeToTxtInShaderVariant(string searchPattern)
        {
            if (_hiFileContext == null)
                return;
            var files = _hiFileContext.GetFiles<ShaderVariantFile>(searchPattern);
            var count = files.Count();
            if (count == 0)
                return;
            var prefService = ServiceProvider.GetRequiredService<IPreferencesService>();
            _pathExport = prefService.Preferences.DefaultExportPath + "\\shader_str\\" + searchPattern + "\\";
            Directory.CreateDirectory(_pathExport);


            foreach (var file in files)
            {

                var sh_bc = getAllShaderByteCodeIn(file);
                using (var progress = ShowProgress())
                {

                    progress.UnitName = sh_bc.Count() > 1 ? "files opened" : "file opened";
                    progress.TotalUnits = sh_bc.Count();
                    progress.IsIndeterminate = sh_bc.Count()==1;

                    var objLock = new object();
                    foreach (var sh in sh_bc)
                    {
                        try
                        {

                            if (File.Exists(_pathExport + sh.FileMemDescriptor.GlobalTagId1 + ".txt"))
                                continue;
                            ShaderBytecodeViewModel temp = new ShaderBytecodeViewModel(ServiceProvider, sh);
                            temp.PropertyChanged += Temp_PropertyChangedAsync;
                            await temp.Initialize();
                        }
                        catch (Exception ex)
                        {

                        }
                        finally
                        {
                            lock (objLock)
                            {
                                progress.CompletedUnits++;
                            }

                        }

                    }


                }


            }
        }


        private List<ShaderBytecodeFile> getAllShaderByteCodeIn(ShaderVariantFile file)
        {
            var root = file.Deserialized?.Root;
            List<ShaderBytecodeFile> result = new List<ShaderBytecodeFile>();

            if (root == null)
                return result;
            ListTagInstance shader_groups = root["shader groups"] as ListTagInstance;
            if (shader_groups == null)
                return result;
            Dictionary<int, ShaderBytecodeFile> dicShaderBC = new Dictionary<int, ShaderBytecodeFile>();
            foreach (var shader_group in shader_groups)
            {
                if (_vertType != -1 && (sbyte)shader_group["vertex type"].AccessValue != _vertType)
                    continue;
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
            if (e.PropertyName == "DecompiledStr" && (sender is ShaderBytecodeViewModel))
            {

                string tempString = (sender as ShaderBytecodeViewModel).DecompiledStr;

                if (string.IsNullOrEmpty(tempString))
                    return;
                if (_onlyVertexShaders && tempString.Contains("; Pixel Shader"))
                    return;
                string tempFilename = _pathExport + Mmr3HashLTU.getMmr3HashFromInt((sender as ShaderBytecodeViewModel).File.FileMemDescriptor.GlobalTagId1) + ".txt";
                FileStream tempStream = new FileStream(tempFilename, FileMode.CreateNew, FileAccess.Write);

                foreach (var item in tempString.ToArray())
                {
                    tempStream.Write(BitConverter.GetBytes(item));
                }

                tempStream.FlushAsync();
                tempStream.Close();

            }
            else if (e.PropertyName == "DecompiledStrGLSL" && (sender is ShaderBytecodeViewModel))
            {
                string tempString = (sender as ShaderBytecodeViewModel).DecompiledStr;
                if (string.IsNullOrEmpty(tempString) || _onlyVertexShaders && tempString.Contains("; Pixel Shader"))
                    return;

                string tempFilename_2 = _pathExport + Mmr3HashLTU.getMmr3HashFromInt((sender as ShaderBytecodeViewModel).File.FileMemDescriptor.GlobalTagId1) + ".gsls";
                FileStream tempStream_2 = new FileStream(tempFilename_2, FileMode.CreateNew, FileAccess.Write);
                string tempString_2 = (sender as ShaderBytecodeViewModel).DecompiledStrGLSL;
                foreach (var item in tempString_2.ToArray())
                {
                    tempStream_2.Write(BitConverter.GetBytes(item));
                }

                tempStream_2.FlushAsync();
                tempStream_2.Close();
            }
        }

        public async Task doSome()
        {
            if (Directory.Exists(TextFilesPath))
            {

                List<string>? list = new List<string>
                {
                    TextFilesPath
                };
                var process = new TextToMmh3LTUProcess(list, Spliters, _forceLowerCase);
                process.Completed += ProcessTextToMmh3_Completed;
                await RunProcess(process);
            }
        }

        private void ProcessTextToMmh3_Completed(object? sender, EventArgs e)
        {
            if (sender is TextToMmh3LTUProcess)
            {
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
