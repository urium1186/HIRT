using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.Processes.Online;
using HaloInfiniteResearchTools.UI.Modals;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using LibHIRT.DAO;
using LibHIRT.Files;
using LibHIRT.Grunt.Models.HaloInfinite;
using Microsoft.Extensions.DependencyInjection;
using OpenSpartan.Grunt.Models.HaloInfinite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HaloInfiniteResearchTools.ViewModels.Online
{

    public enum CustomOnlineSubView
    {
        Login,
        ItemList
    }

    public class CoreExtraData
    {
        public string Name { get; set; }

    }

    public class DataGridRowData
    {
        private string imagen;
        public string Imagen
        {
            get { return imagen; }
            set
            {
                imagen = value;
                if (!string.IsNullOrEmpty(imagen))
                    ImagenSource = new BitmapImage(new Uri(imagen));
            }
        }

        public ImageSource ImagenSource { get; set; }
        public string Texto { get; set; }
        public ArmorCoreThemeExtende Theme { get; set; }
    }

    public class CustomOnlineViewModel : ViewModel, IDisposeWithView
    {
        private static BitmapImage _defaultImage = new(new Uri("pack://application:,,,/HaloInfiniteResearchTools;component/Resources/images/item_loading.jpeg"));
        public static BitmapImage DefaultImage { get => _defaultImage; }

        private int _selectedTabIndex;
        private OnlineSQLiteDriver onlineSQLiteDriver;
        private string _imagenPath;
        public string ImagenPath
        {
            get { return _imagenPath; }
            set
            {
                _imagenPath = value;
                OnPropertyChanged("ImagenPath");
            }
        }
        private ImageSource _prevImageSource = DefaultImage;
        public ImageSource PrevImageSource
        {
            get { return _prevImageSource; }
            set
            {
                if (value == null)
                {
                    _prevImageSource = DefaultImage;
                }
                else
                {
                    _prevImageSource = value;
                }

                OnPropertyChanged("PrevImageSource");
            }
        }
        public List<DataGridRowData> GetOnDbThems
        {
            get
            {
                return getGetOnDbThemsOnSelect();
            }
        }
        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }
            set
            {
                _selectedTabIndex = value;
                // Notifica que la propiedad ha cambiado, dependiendo de cómo esté implementada tu clase ViewModel
                OnPropertyChanged(nameof(SelectedTabIndex));
                OnPropertyChanged(nameof(SelectedTabExtra));
            }
        }

        public CoreExtraData SelectedTabExtra
        {
            get
            {
                if (_coreItemsName != null)
                {
                    return _coreItemsName[_coreItemsName.Keys.ElementAt(_selectedTabIndex)];
                }
                return new CoreExtraData();
            }
        }

        public ArmorCore SelectedCore
        {
            get
            {
                if (_coreItems != null)
                {
                    return _coreItems.Keys.ElementAt(_selectedTabIndex);
                }
                return new ArmorCore();
            }
        }
        public Dictionary<string, OnlineItemModel> ItemsSelectedCore
        {
            get
            {
                if (_coreItems != null)
                {
                    var k = _coreItems.Keys.ElementAt(_selectedTabIndex);
                    var theme = _coreItems[k];
                    if (theme == null)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            GetFromXboxWebArmorCoreThemeProcess(k);
                        });

                    }
                    return _coreItems[k];
                }
                return new Dictionary<string, OnlineItemModel>();
            }
        }
        Dictionary<string, OnlineItemModel> selectedRow = null;
        public Dictionary<string, OnlineItemModel> ItemsRowSelectedTheme
        {
            get
            {
                if (selectedRow != null)
                {
                    return selectedRow;
                }
                return new Dictionary<string, OnlineItemModel>();
            }
        }

        private async void CapturarPantalla()
        {

            var modal = ServiceProvider.GetService<GetImageModal>();
            //modal.DataContext = new ;
            modal.ModalClosing += Modal_ModalClosing;

            Modals.Add(modal);
            await modal.Show();
            IsBusy = true;
            //Modals.Remove(modal);



            return;
            /*await Task.Delay(TimeSpan.FromSeconds(3));

            using (var bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                    string tempPath = System.IO.Path.GetTempPath();
                    ImagenPath = System.IO.Path.Combine(tempPath, "screenshot.png");
                    if (System.IO.File.Exists(ImagenPath))
                    {
                        System.IO.File.Delete(ImagenPath);
                    }
                    bitmap.Save(ImagenPath, ImageFormat.Png);
                }
            }*/
        }
        private List<DataGridRowData> getGetOnDbThemsOnSelect()
        {
            string core_id = _coreItemsName.Keys.ElementAt(_selectedTabIndex);
            List<ArmorCoreThemeExtende> db_result = onlineSQLiteDriver.getOnOwnThemes(core_id);
            List<DataGridRowData> result = new List<DataGridRowData>();
            foreach (var item in db_result)
            {
                var images = onlineSQLiteDriver.GetImage(item.id);
                ImageSource imageSource = null;
                if (images != null && images.Count > 0)
                {
                    imageSource = GetImageModal.BytesToImageSource(images.Last());

                }
                if (imageSource == null)
                {
                    string imagen = "pack://application:,,,/HaloInfiniteResearchTools;component/Resources/images/item_loading.jpeg";
                    imageSource = new BitmapImage(new Uri(imagen));
                }
                var temp = new DataGridRowData();
                temp.ImagenSource = imageSource;
                temp.Texto = item.ThemeName;
                temp.Theme = item;
                result.Add(
                    temp
                );
            }
            return result;
        }

        private HIFileContext _hiFileContext;
        CustomOnlineSubView currentView = CustomOnlineSubView.Login;

        private Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>> _coreItems;
        private Dictionary<string, CoreExtraData> _coreItemsName;
        private ArmorCore armorCoreToSave;

        public Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>> CoreItems { get => _coreItems; set => _coreItems = value; }

        public bool HaveCredentials
        {
            get => _hiFileContext != null && _hiFileContext.ConnectXbox != null;
        }

        public bool ShowLogin { get { return currentView == CustomOnlineSubView.Login; } }
        public bool ShowList { get { return currentView == CustomOnlineSubView.ItemList; } }


        public ItemType ItemTypeSel { get; set; }

        public ICommand ProcessLoginCommand { get; }
        public ICommand GetArmorCoresCommand { get; }
        public ICommand GetArmorCoreCommand { get; }
        public ICommand WebApiItemsCommand { get; }
        public ICommand WebApiStoresCommand { get; }
        public ICommand SaveCoreToDbCommand { get; }
        public ICommand SaveThemeToCmsCommand { get; }
        public ICommand DeleteThemeToCmsCommand { get; }
        public ICommand UpdateThemeToCmsCommand { get; }
        public ICommand PreviewThemeToCmsCommand { get; }

        public ICommand OnGridChangeSelectionCommand { get; private set; }
        public ICommand CapturarPantallaCommand { get; private set; }
        public ICommand SeleccionarImagenCommand { get; private set; }
        public Dictionary<string, CoreExtraData> CoreItemsName { get => _coreItemsName; set => _coreItemsName = value; }

        public CustomOnlineViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _hiFileContext = HIFileContext.Instance;
            onlineSQLiteDriver = new OnlineSQLiteDriver();
            ProcessLoginCommand = new Command(ProcessLogin);
            GetArmorCoresCommand = new Command(GetArmorCores);
            GetArmorCoreCommand = new Command<ArmorCore>(GetArmorCore);
            WebApiItemsCommand = new Command(GetApiItems);
            WebApiStoresCommand = new Command(GetApiStores);
            SaveThemeToCmsCommand = new Command<ArmorCoreThemeExtende>(SaveThemeToCms);
            DeleteThemeToCmsCommand = new Command<ArmorCoreThemeExtende>(DeleteThemeToCms);
            UpdateThemeToCmsCommand = new Command<ArmorCoreThemeExtende>(UpdateThemeToCms);
            PreviewThemeToCmsCommand = new Command<ArmorCoreThemeExtende>(PreviewThemeToCms);
            OnGridChangeSelectionCommand = new Command<DataGridRowData>(OnGridChangeSelection);
            CapturarPantallaCommand = new Command(CapturarPantalla);
            SeleccionarImagenCommand = new Command(SeleccionarImagen);
            SaveCoreToDbCommand = new Command<ArmorCore>(SaveCoreToDb);
            _coreItemsName = new Dictionary<string, CoreExtraData>();
            _coreItems = new Dictionary<ArmorCore, Dictionary<string, OnlineItemModel>>();
            /*
             * DataGridRowData
            for (int i = 0; i < 25; i++)
            {
                OnlineItemModel temp = new OnlineItemModel();
                temp.Description = "Prueba " + i.ToString();
                if (i%2==0)
                    temp.ImageSource = @"C:\Users\Jorge\HIRT\json\progression\Inventory\Armor\Coatings\002-001-fwl-24b01ec3-SM.png";
                _coreItems.Add(temp);
            }
            */

        }

        private void OnGridChangeSelection(DataGridRowData data)
        {
            if (data != null)
            {
                PrevImageSource = data.ImagenSource;
            }
            else
            {
                PrevImageSource = null;
            }
            selectedRow = null;
            OnPropertyChanged("ItemsRowSelectedTheme");
        }

        private async void PreviewThemeToCms(ArmorCoreThemeExtende obj)
        {
            try
            {
                var process = new XboxWebApiArmorCoresProcess(_hiFileContext.ConnectXbox, armorCoresAction: XboxWebApiArmorCoresProcess.XboxWebApiArmorCoresAction.LoadItemsOfTheme);
                process.Args = new object[] { (ArmorCoreTheme)obj };
                await RunProcess(process, true);
                if (process.Result != null && process.Result.Count > 0)
                {
                    var k = process.Result.Keys.ElementAt(0);
                    selectedRow = process.Result[k];
                    OnPropertyChanged("ItemsRowSelectedTheme");

                }

            }
            catch (Exception ex)
            {

            }
            finally
            {

            }


        }

        private void UpdateThemeToCms(ArmorCoreThemeExtende obj)
        {
            //onlineSQLiteDriver.insertOnArmorCore(armorCoreToSave, SelectedTabExtra.Name, data);
            OnPropertyChanged("GetOnDbThems");
        }

        private void DeleteThemeToCms(ArmorCoreThemeExtende obj)
        {
            if (obj != null)
            {
                onlineSQLiteDriver.DeleteCustomArmorCoresOwnThemes(obj.id);
                OnPropertyChanged("GetOnDbThems");
            }

        }

        private void GetArmorCore(ArmorCore core)
        {
            GetArmorCoreTask(core.CoreId);
        }

        private async void SaveThemeToCms(ArmorCoreThemeExtende core)
        {
            ArmorCoreTheme theme = (ArmorCoreTheme)core;
            ArmorCore temp = new ArmorCore();
            ArmorCore selected = SelectedCore;
            temp.CorePath = selected.CorePath;
            temp.CoreType = selected.CoreType;
            temp.CoreId = selected.CoreId;
            temp.FirstAcquiredDate = selected.FirstAcquiredDate;
            temp.IsEquipped = selected.IsEquipped;
            temp.Themes = new List<ArmorCoreTheme>
            {
                theme
            };
            PutArmorCoreTask(temp);
        }


        private void Modal_ModalClosing(object? sender, Modal e)
        {
            Modals.Remove(e);
            IsBusy = false;
            byte[] data = GetImageModal.ImageSourceToBytes(((GetImageModal)e).ScreenCaptureImageSource);
            onlineSQLiteDriver.insertOnArmorCore(armorCoreToSave, SelectedTabExtra.Name, data);
            OnPropertyChanged("GetOnDbThems");
        }

        private void SeleccionarImagen()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Archivos de imagen (*.png;*.jpg)|*.png;*.jpg";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ImagenPath = dialog.FileName;
            }
        }
        private void SaveCoreToDb(ArmorCore arg)
        {
            armorCoreToSave = arg;
            if (string.IsNullOrEmpty(SelectedTabExtra.Name))
                return;
            CapturarPantalla();

        }

        private async void ProcessLogin()
        {
            using (var progress = ShowSpinner())
            {
                progress.IsIndeterminate = true;
                progress.Status = "Login";

                var objLock = new object();


                try
                {
                    var process = new ConnectXboxServicesProcess();
                    process.Completed += ConnectXboxServicesProcess_Completed;
                    await RunProcess(process, true);

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
            _hiFileContext.ConnectXbox = (sender as ConnectXboxServicesProcess).Result;
            OnPropertyChanged("HaveCredentials");
            App.Current.Dispatcher.Invoke(() =>
            {
                GetArmorCores();// Código que accede a los componentes de la interfaz de usuario
            });

        }

        private async void GetArmorCores()
        {
            //throw new NotImplementedException();
            using (var progress = ShowSpinner())
            {
                progress.IsIndeterminate = true;
                progress.Status = "Loading Armor Cores";

                var objLock = new object();


                try
                {
                    var process = new XboxWebApiArmorCoresProcess(_hiFileContext.ConnectXbox);
                    process.Completed += GetFromXboxWebApiProcess_Completed;
                    await RunProcess(process, true);

                }
                catch (Exception ex)
                {

                }
                finally
                {
                    lock (objLock)
                    {
                        progress.CompletedUnits++; //GetFromXboxWebApiItemsProcess
                    }

                }



            }
        }

        private async void PutArmorCoreTask(ArmorCore armorCore)
        {
            //throw new NotImplementedException();
            using (var progress = ShowSpinner())
            {
                progress.IsIndeterminate = true;
                progress.Status = "Loading Armor Core";

                var objLock = new object();


                try
                {
                    var process = new XboxWebApiArmorCoresProcess(_hiFileContext.ConnectXbox, armorCoresAction: XboxWebApiArmorCoresProcess.XboxWebApiArmorCoresAction.PutCurrentArmorCore);
                    process.Args = new object[] { armorCore };
                    await RunProcess(process, true);
                    if (process.Result != null && process.Result.Count > 0)
                    {
                        var k = process.Result.Keys.ElementAt(0);
                        GetFromXboxWebArmorCoreThemeProcess(k);
                    }

                }
                catch (Exception ex)
                {

                }
                finally
                {
                    lock (objLock)
                    {
                        progress.CompletedUnits++; //GetFromXboxWebApiItemsProcess
                    }

                }



            }
        }
        private async void GetArmorCoreTask(string id)
        {
            //throw new NotImplementedException();
            using (var progress = ShowSpinner())
            {
                progress.IsIndeterminate = true;
                progress.Status = "Loading Armor Core";

                var objLock = new object();


                try
                {
                    var process = new XboxWebApiArmorCoresProcess(_hiFileContext.ConnectXbox, armorCoresAction: XboxWebApiArmorCoresProcess.XboxWebApiArmorCoresAction.GetCurrentArmorCore);
                    process.Args = new object[] { id };
                    await RunProcess(process, true);
                    if (process.Result != null && process.Result.Count > 0)
                    {
                        var k = process.Result.Keys.ElementAt(0);
                        GetFromXboxWebArmorCoreThemeProcess(k);
                    }

                }
                catch (Exception ex)
                {

                }
                finally
                {
                    lock (objLock)
                    {
                        progress.CompletedUnits++; //GetFromXboxWebApiItemsProcess
                    }

                }



            }
        }

        private async void GetFromXboxWebArmorCoreThemeProcess(ArmorCore core)
        {
            try
            {
                var process = new XboxWebApiArmorCoresProcess(_hiFileContext.ConnectXbox, armorCoresAction: XboxWebApiArmorCoresProcess.XboxWebApiArmorCoresAction.LoadItemsOfArmorCore);
                process.Args = new object[] { core };
                await RunProcess(process, true);
                if (process.Result != null && process.Result.Count > 0)
                {
                    var k = process.Result.Keys.ElementAt(0);
                    for (int i = 0; i < this._coreItems.Count; i++)
                    {
                        var item_key = this._coreItems.Keys.ElementAt(i);
                        if (item_key.CoreId == k.CoreId)
                        {
                            this._coreItems[item_key] = process.Result[k];
                            item_key.Themes = k.Themes;
                            OnPropertyChanged("ItemsSelectedCore");
                            break;
                        }

                    }

                }

            }
            catch (Exception ex)
            {

            }
            finally
            {

            }

        }

        private async void GetApiItems()
        {
            //throw new NotImplementedException();
            using (var progress = ShowProgress())
            {
                progress.IsIndeterminate = true;
                progress.Status = "Login";

                var objLock = new object();


                try
                {
                    var process = new GetFromXboxWebApiItemsProcess(_hiFileContext.ConnectXbox, ItemTypeSel);
                    //var process = new GetFromXboxWebApiStoreProcess(_hiFileContext.ConnectXbox);

                    //process.Completed += GetFromXboxWebApiProcess_Completed;
                    await RunProcess(process, true);

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

        private async void GetApiStores()
        {
            //throw new NotImplementedException();
            using (var progress = ShowProgress())
            {
                progress.IsIndeterminate = true;
                progress.Status = "Login";

                var objLock = new object();


                try
                {
                    //var process = new GetFromXboxWebApiItemsProcess(_hiFileContext.ConnectXbox, ItemTypeSel);
                    var process = new GetFromXboxWebApiStoreProcess(_hiFileContext.ConnectXbox);

                    //process.Completed += GetFromXboxWebApiProcess_Completed;
                    await RunProcess(process, true);

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

            App.Current.Dispatcher.Invoke(() =>
            {
                XboxWebApiArmorCoresProcess process = sender as XboxWebApiArmorCoresProcess;
                process.Completed -= GetFromXboxWebApiProcess_Completed;
                if (process != null)
                {
                    var r = process.Result;

                    if (r != null && r.Count > 0)
                    {
                        _coreItemsName.Clear();
                        foreach (var item in r.Keys)
                        {
                            _coreItemsName[item.CoreId] = new CoreExtraData
                            {
                                Name = ""
                            };
                        }
                        this.CoreItems = r;
                        SelectedTabIndex = 0;
                        OnPropertyChanged("CoreItems");
                        OnPropertyChanged("ItemsSelectedCore");
                    }
                    this.currentView = CustomOnlineSubView.ItemList;
                    OnPropertyChanged("ShowList");

                    OnPropertyChanged("ShowLogin");
                }
            });

        }
    }
}
