using HaloInfiniteResearchTools.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.Services.Abstract;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using HaloInfiniteResearchTools.Views;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using MaterialCore = HelixToolkit.SharpDX.Core.Model.MaterialCore;
using PhongMaterialCore = HelixToolkit.SharpDX.Core.Model.PhongMaterialCore;
using TextureModel = HelixToolkit.SharpDX.Core.TextureModel;
using LibHIRT.Processes;
using LibHIRT.Domain.RenderModel;
using HaloInfiniteResearchTools.Controls;
using LibHIRT.TagReader;
using SharpDX.Win32;
using System.Xml.Linq;
using System.Windows;
using System.Diagnostics;
using LibHIRT.Domain;
using System.Net.Mail;

namespace HaloInfiniteResearchTools.ViewModels
{

    [AcceptsFileType(typeof(RenderModelFile))]
    public class RenderModelViewModel : ViewModel, IDisposeWithView
    {

        #region Data Members

        private readonly IHIFileContext _fileContext;
        private readonly IMeshIdentifierService _meshIdentifierService;

        private SSpaceFile _file;

        private string _searchTerm;
        private ObservableCollection<ModelNodeModel> _nodes;
        private ICollectionView _nodeCollectionView;

        private Assimp.Scene _assimpScene;
        private RenderModelDefinition _renderModelDef;
        private Dictionary<string, TextureModel> _loadedTextures;
        private ObservableCollection<TreeViewItemModel> _regions;
        private ObservableCollection<TreeViewItemModel> _variants;
        private ObservableCollection<TreeViewItemModel> _themeConfigurations;

        #endregion

        #region Properties

        public ModelViewerOptionsModel Options { get; set; }

        public Camera Camera { get; set; }
        public SceneNodeGroupModel3D Model { get; }
        public EffectsManager EffectsManager { get; }
        public Viewport3DX Viewport { get; set; }

        public double MinMoveSpeed { get; set; }
        public double MoveSpeed { get; set; }
        public double MaxMoveSpeed { get; set; }

        [OnChangedMethod(nameof(ToggleShowWireframe))]
        public bool ShowWireframe { get; set; }
        [OnChangedMethod(nameof(ToggleShowTextures))]
        public bool ShowTextures { get; set; }
        public bool UseFlycam { get; set; }

        public ICollectionView Nodes => _nodeCollectionView;



        public int MeshCount { get; set; }
        public int VertexCount { get; set; }
        public int FaceCount { get; set; }

        public ICommand SearchTermChangedCommand { get; }

        public ICommand ShowAllCommand { get; }
        public ICommand HideAllCommand { get; }
        public ICommand HideLODsCommand { get; }
        public ICommand HideVolumesCommand { get; }
        public ICommand ExpandAllCommand { get; }
        public ICommand CollapseAllCommand { get; }

        public ICommand ExportModelCommand { get; }
        public ObservableCollection<TreeViewItemModel> Regions { get => _regions; set => _regions = value; }
        public ModelInfoToRM ModelInfo { get; set; }
        public ListTagInstance ThemeConfigurations { get; set; }
        public ObservableCollection<TreeViewItemModel> Variants { get => _variants; set => _variants = value; }
        public ObservableCollection<TreeViewItemModel> ThemeConfigurationsTVIM {
            get => _themeConfigurations;
            set => _themeConfigurations = value; }

        #endregion

        #region Constructor

        public RenderModelViewModel(IServiceProvider serviceProvider, SSpaceFile file)
          : base(serviceProvider)
        {
            _file = file;
            _loadedTextures = new Dictionary<string, TextureModel>();

            _fileContext = ServiceProvider.GetRequiredService<IHIFileContext>();
            _meshIdentifierService = ServiceProvider.GetRequiredService<IMeshIdentifierService>();

            EffectsManager = new DefaultEffectsManager();
            Camera = new PerspectiveCamera() { FarPlaneDistance = 300000 };
            Model = new SceneNodeGroupModel3D();
            ApplyTransformsToModel();

            _nodes = new ObservableCollection<ModelNodeModel>();
            _nodeCollectionView = InitializeNodeCollectionView(_nodes);

            _regions = new ObservableCollection<TreeViewItemModel>();

            _variants = new ObservableCollection<TreeViewItemModel>();
            _themeConfigurations = new ObservableCollection<TreeViewItemModel>();

            ShowTextures = true;

            ShowAllCommand = new Command(ShowAllNodes);
            HideAllCommand = new Command(HideAllNodes);
            HideLODsCommand = new Command(HideLODNodes);
            HideVolumesCommand = new Command(HideVolumeNodes);
            ExpandAllCommand = new Command(ExpandAllNodes);
            CollapseAllCommand = new Command(CollapseAllNodes);
            SearchTermChangedCommand = new Command<string>(SearchTermChanged);

            ExportModelCommand = new AsyncCommand(ExportModel);
        }

        #endregion

        #region Overrides

        protected override async Task OnInitializing()
        {
            Options = GetPreferences().ModelViewerOptions;
            UseFlycam = Options.DefaultToFlycam;

            var convertProcess = new ConvertModelToAssimpSceneProcess(_file);
            await RunProcess(convertProcess);

            _assimpScene = convertProcess.Result;


            _renderModelDef = convertProcess.RenderModelDef;

            using (var prog = ShowProgress())
            {
                prog.Status = "Preparing Viewer";
                prog.IsIndeterminate = true;

                await PrepareModelViewer(convertProcess.Result);
            };
        }

        protected override void OnDisposing()
        {
            Model?.Dispose();
            EffectsManager?.DisposeAllResources();
            GCHelper.ForceCollect();
        }

        #endregion

        #region Private Methods

        private ICollectionView InitializeNodeCollectionView(ObservableCollection<ModelNodeModel> files)
        {
            var collectionView = CollectionViewSource.GetDefaultView(_nodes);
            collectionView.SortDescriptions.Add(new SortDescription(nameof(ModelNodeModel.Name), ListSortDirection.Ascending));
            collectionView.Filter = (obj) =>
            {
                if (string.IsNullOrEmpty(_searchTerm))
                    return true;

                var node = obj as ModelNodeModel;
                return node.Name.Contains(_searchTerm, StringComparison.InvariantCultureIgnoreCase);
            };

            return collectionView;
        }

        private async Task<TextureModel> LoadTexture(string name)
        {

            if (string.IsNullOrWhiteSpace(name))
                return null;

            name = Path.ChangeExtension(name, ".pct");
            if (_loadedTextures.TryGetValue(name, out var texture))
                return texture;

            var file = _fileContext.GetFile<PictureFile>(name);
            if (file is null)
                return null;

            var svc = ServiceProvider.GetService<ITextureConversionService>();
            var stream = await svc.GetJpgStream(file, Options.ModelTexturePreviewQuality);

            texture = TextureModel.Create(stream);
            _loadedTextures.Add(name, texture);
            return texture;
            //return null;
        }

        private async void ApplyTexturesToNode(MeshNode meshNode)
        {
            var nodeMaterial = meshNode.Material as PhongMaterialCore;
            if (nodeMaterial is null)
                return;

            var baseTexName = nodeMaterial.DiffuseMapFilePath;
            if (string.IsNullOrWhiteSpace(baseTexName))
                return;

            nodeMaterial.DiffuseMap = await LoadTexture($"{baseTexName}.pct");
            nodeMaterial.SpecularColorMap = await LoadTexture($"{baseTexName}_spec.pct");
            //nodeMaterial.NormalMap = await LoadTexture( $"{baseTexName}_nm.pct" );
            nodeMaterial.EnableTessellation = true;
            nodeMaterial.UVTransform = new UVTransform(0, 1, -1, 0, 0);
        }

        private void ApplyTransformsToModel()
        {
            var transformGroup = new System.Windows.Media.Media3D.Transform3DGroup();

            var rotTransform = new System.Windows.Media.Media3D.RotateTransform3D();
            rotTransform.Rotation = new System.Windows.Media.Media3D.AxisAngleRotation3D(
              new System.Windows.Media.Media3D.Vector3D(0, 1, 0), 90);
            transformGroup.Children.Add(rotTransform);

            var scaleTransform = new System.Windows.Media.Media3D.ScaleTransform3D(100, 100, 100);
            //transformGroup.Children.Add( scaleTransform );

            Model.Transform = transformGroup;
        }

        private async Task PrepareModelViewer(Assimp.Scene assimpScene)
        {
            var importer = new Importer();
            importer.ToHelixToolkitScene(assimpScene, out var scene);

            void AddNodeModels(SceneNode node)
            {
                if (node is MeshNode meshNode)
                {
                    meshNode.CullMode = SharpDX.Direct3D11.CullMode.Back;

                    var nodeModel = new ModelNodeModel(node);
                    _nodes.Add(nodeModel);
                }

                foreach (var childNode in node.Items)
                    AddNodeModels(childNode);
            }

            AddNodeModels(scene.Root);
            FillRegionsList();
            FillVariantsList();
            FillThemeConfigurationList();
            var materialLookup = new Dictionary<string, Material>();
            foreach (var node in scene.Root.Traverse())
            {
                if (node is MeshNode meshNode)
                    ApplyTexturesToNode(meshNode);
            }

            if (Options.DefaultHideLODs)
                HideLODNodes();

            if (Options.DefaultHideVolumes)
                HideVolumeNodes();

            App.Current.Dispatcher.Invoke(() =>
            {
                Model.AddNode(scene.Root);
                CalculateMoveSpeed(scene);
                UpdateMeshInfo();
            });

            await Task.Delay(250).ContinueWith(t =>
            {
                App.Current.Dispatcher.Invoke(() => Camera.ZoomExtents(Viewport));
            });
        }

        private void FillVariantsList() {
            if (ModelInfo.Variants != null) {
                foreach (var item in ModelInfo.Variants)
                {

                    var style = (item["style"] as Mmr3Hash).Str_value;
                    var runtimeVariantRegionIndices = (item["runtime variant region indices"] as ArrayFixLen);

                    var damageStyleIndex = (item["Damage Style Index"] as FourByte);
                    TreeViewItemModel temp1 = GetMeshInVariant(item, _renderModelDef,ModelInfo);
                    _variants.Add(temp1);
                }
                //_variants
            }

            
        }
        TreeViewItemModel GetMeshInVariant(TagInstance item, RenderModelDefinition renderModelDef, ModelInfoToRM modelInfoToRM)
        {
            var name = (item["name"] as Mmr3Hash).Str_value;
            var temp1 = new TreeViewItemModel();
            temp1.Header = name.ToString();
            var regions = (item["regions"] as Tagblock);
            foreach (var region in regions)
            {
                var region_name = (region["region name"] as Mmr3Hash).Str_value;
                var runtime_region_index = (region["runtime region index"]).AccessValue;
                var parent_variant = (region["parent variant"]).AccessValue;
                var permutations = (region["permutations"] as Tagblock);
                var temp2 = new TreeViewItemModel();
                temp2.Header = (region["region name"] as Mmr3Hash).Str_value.ToString();
                temp2.SetValue(ItemHelper.ParentProperty, temp1);
                temp1.Children.Add(temp2);
                foreach (var permutation in permutations)
                {
                    var permutation_name = (permutation["permutation name"] as Mmr3Hash).Str_value;
                    var runtime_permutation_index = (permutation["runtime permutation index"]).AccessValue;
                    if ((SByte)runtime_region_index == -1)
                        continue;
                    var runtimeRegI = modelInfoToRM.RuntimeRegions[(SByte)runtime_region_index];
                    var runtimeRegName = (runtimeRegI["name"] as Mmr3Hash).Value;
                    var runtimeRegpermutations = (runtimeRegI["permutations"] as Tagblock);
                    if ((int)runtime_permutation_index == -1)
                        continue;
                    var runtimeRegpermutationsI = runtimeRegpermutations[(int)runtime_permutation_index];
                    var runtimeRegpermutationsI_Name = (runtimeRegpermutationsI["name"] as Mmr3Hash).Value;
                    var temp3 = new TreeViewItemModel();
                    temp3.Header = (runtimeRegpermutationsI["name"] as Mmr3Hash).Str_value.ToString();
                    temp3.SetValue(ItemHelper.ParentProperty, temp2);
                    temp2.Children.Add(temp3);
                    getMeshIndexByRegionPermName(runtimeRegName, runtimeRegpermutationsI_Name, renderModelDef, ref temp3);


                }
            }

            return temp1;
        }

        TreeViewItemChModel getMeshIndexByRegionPermName(int regionNameId, int permuNameId,RenderModelDefinition renderModelDef, ref TreeViewItemModel parent) {
            foreach (var region in renderModelDef.Regions)
            {
                if (region.name_id == regionNameId) {
                    foreach (var perm in region.permutations)
                    {
                        if (perm.name_id == permuNameId) {
                            for (int k = perm.mesh_index; k < perm.mesh_index + perm.mesh_count; k++)
                            {
                                var temp3 = new TreeViewItemChModel();
                                temp3.Header = k.ToString();
                                temp3.SetValue(ItemHelper.ParentProperty, parent);
                                temp3.Value = _nodes[k];
                                parent.Children.Add(temp3);
                            }
                        }
                    }
                }
            }
            return null;
        }
        private void FillThemeConfigurationList() {
            if (ThemeConfigurations == null)
                return;

            foreach (var themeConfiguration in ThemeConfigurations)
            {
                var temp = new TreeViewItemModel();
                temp.Header = themeConfiguration["Theme Name"].AccessValue.ToString();
                var result = HIFileContext.GetFileFrom(themeConfiguration["Theme Configs"] as TagRef, _file.Parent as ModuleFile);
                if (result != null)
                {
                    var r = result as ObjectCustomizationThemeConfiguration;
                    #region Regions

                    ReadMeshCoustoms(r, ref temp, "Regions");

                    #endregion
                    #region Attachments
                    
                    ReadMeshAttachments(r, ref temp);
                    
                    #endregion

                    #region Prosthetics
                    ReadMeshCoustoms(r, ref temp, "Prosthetics");
                    #endregion

                    #region Body Types
                    ReadMeshCoustoms(r, ref temp, "Body Types");
                    #endregion
                }
                _themeConfigurations.Add(temp);
            }
        }

        private void ReadMeshCoustoms(ObjectCustomizationThemeConfiguration? r, ref TreeViewItemModel parent, string part)
        {
            TreeViewItemModel tempPart = new TreeViewItemModel();
            tempPart.Header = part;
            var items = r.Deserialized?[part] as ListTagInstance;
            foreach (var item in items)
            {
                var temp1 = new TreeViewItemModel();
                string name = part == "Regions" ? "Region Name" : "Name";

                temp1.Header = item[name].AccessValue.ToString();
                ListTagInstance p_r_l = item["Permutation Regions"] as ListTagInstance;
                ListTagInstance p_s_l = item["Permutation Settings"] as ListTagInstance;
                foreach (var p_r in p_r_l)
                {
                    var temp2 = new TreeViewItemModel();
                    temp2.Header = p_r["Permutation Region"].AccessValue.ToString();

                    foreach (var p_s in p_s_l)
                    {
                        var temp3 = new TreeViewItemModel();
                        temp3.Header = p_s["Permutation Name"].AccessValue.ToString();
                        List<TreeViewItemChModel> meshs = GetPermutationBy((int)p_r["Permutation Region"].AccessValue, (int)p_s["Permutation Name"].AccessValue);
                        if (meshs != null)
                        {
                            foreach (var mesh in meshs)
                            {
                                mesh.SetValue(ItemHelper.ParentProperty, temp3);
                                temp3.Children.Add(mesh);
                            }
                        }
                        temp3.SetValue(ItemHelper.ParentProperty, temp2);
                        temp2.Children.Add(temp3);
                    }
                    temp2.SetValue(ItemHelper.ParentProperty, temp1);
                    temp1.Children.Add(temp2);
                }
                temp1.SetValue(ItemHelper.ParentProperty, tempPart);
                tempPart.Children.Add(temp1);
            }
            tempPart.SetValue(ItemHelper.ParentProperty, parent);
            parent.Children.Add(tempPart);
        }
        private void ReadMeshAttachments(ObjectCustomizationThemeConfiguration? r, ref TreeViewItemModel parent) {
            TreeViewItemModel tempPart = new TreeViewItemModel();
            tempPart.Header = "Attachments";
            var items = r.Deserialized?[tempPart.Header] as ListTagInstance;
            foreach (var item in items)
            {
                
                TagRef tagRef = item["Attachment"] as TagRef;
                if (tagRef == null || tagRef.Ref_id_int == -1)
                    continue;
                CustomizationAttachmentConfiguration attachFile = HIFileContext.GetFileFrom(tagRef) as CustomizationAttachmentConfiguration;
                if (attachFile == null)
                    continue;
                ListTagInstance tgl = attachFile.Deserialized.Root["Model Attachments"] as ListTagInstance;
                if (tgl == null || tgl.Count == 0)
                    continue;

                foreach (var modelAttachment in tgl)
                {
                    TagRef attach_model_ref = modelAttachment["Attachment Model"] as TagRef;
                    if (attach_model_ref == null || attach_model_ref.TagGroupRev != "hlmt")
                        continue;
                    ModelFile modelFile = HIFileContext.GetFileFrom(attach_model_ref) as ModelFile;
                    if (modelFile == null)
                        continue;
                    var rmf = modelFile.GetRenderModel();
                    if (rmf == null)
                        continue;
                    var convertProcess = new ConvertModelToAssimpSceneProcess(rmf);
                    
                    RenderModelDefinition temp_renderModelDef = null;
                    try
                    {
                        RunProcess(convertProcess).Wait(250);
                        temp_renderModelDef = convertProcess.RenderModelDef;
                    }
                    catch (Exception exi)
                    {
                        temp_renderModelDef = convertProcess.RenderModelDef;

                    }
                    
                    


                   
                    if (temp_renderModelDef == null)
                        continue;

                    ModelInfoToRM vara_reg = modelFile.GetModelVariants();
                    bool found = false;
                    TreeViewItemModel temp_tvm = null;
                    foreach (var item_variant in vara_reg.Variants)
                    {
                        if ((item_variant["name"] as Mmr3Hash).AccessValue.ToString() == modelAttachment["Variant"].AccessValue.ToString()) { 
                            found = true;
                            temp_tvm = GetMeshInVariant(item_variant, temp_renderModelDef, vara_reg);


                            break;
                        }

                    }
                    if (!found)
                        continue;
                    TreeViewItemModel temp1 = new TreeViewItemModel
                    {
                        Header = modelAttachment["Variant"].AccessValue.ToString()
                    };
                    /*
                    ListTagInstance p_r_l = item["Permutation Regions"] as ListTagInstance;
                    ListTagInstance p_s_l = item["Permutation Settings"] as ListTagInstance;
                    foreach (var p_r in p_r_l)
                    {
                        var temp2 = new TreeViewItemModel();
                        temp2.Header = p_r["Permutation Region"].AccessValue.ToString();

                        foreach (var p_s in p_s_l)
                        {
                            var temp3 = new TreeViewItemModel();
                            temp3.Header = p_s["Permutation Name"].AccessValue.ToString();
                            List<TreeViewItemChModel> meshs = GetPermutationBy((int)p_r["Permutation Region"].AccessValue, (int)p_s["Permutation Name"].AccessValue);
                            if (meshs != null)
                            {
                                foreach (var mesh in meshs)
                                {
                                    mesh.SetValue(ItemHelper.ParentProperty, temp3);
                                    temp3.Children.Add(mesh);
                                }
                            }
                            temp3.SetValue(ItemHelper.ParentProperty, temp2);
                            temp2.Children.Add(temp3);
                        }
                        temp2.SetValue(ItemHelper.ParentProperty, temp1);
                        temp1.Children.Add(temp2);
                    }*/
                    temp_tvm.SetValue(ItemHelper.ParentProperty, tempPart);
                    tempPart.Children.Add(temp_tvm);
                }
            }
            tempPart.SetValue(ItemHelper.ParentProperty, parent);
            parent.Children.Add(tempPart);
        }
        private List<TreeViewItemChModel> GetPermutationBy(int regName, int perName) {
            for (int i = 0; i < _renderModelDef.Regions.Length; i++)
            {
                if (_renderModelDef.Regions[i].name_id == regName)
                {
                    for (int j = 0; j < _renderModelDef.Regions[i].permutations.Length; j++)
                    {
                        if (_renderModelDef.Regions[i].permutations[j].name_id == perName) {
                            var perm = _renderModelDef.Regions[i].permutations[j];
                            List <TreeViewItemChModel> result = new List<TreeViewItemChModel>();
                            for (int k = perm.mesh_index; k < perm.mesh_index + perm.mesh_count; k++)
                            {
                                var temp = new TreeViewItemChModel();
                                temp.Header = k.ToString();
                                temp.Value = _nodes[k];
                                result.Add(temp);
                            }
                            return result;
                        }
                    }
                    break;
                }
            }
            return null;
        }
        private void FillRegionsList()
        {
            for (int i = 0; i < _renderModelDef.Regions.Length; i++)
            {
                
                var temp = new TreeViewItemModel();
                temp.Header = _renderModelDef.Regions[i].name_id.ToString();
                for (int j = 0; j < _renderModelDef.Regions[i].permutations.Length; j++)
                {
                    var temp2 = new TreeViewItemModel();
                    temp2.Header = _renderModelDef.Regions[i].permutations[j].name_id.ToString();
                    var perm = _renderModelDef.Regions[i].permutations[j];
                    for (int k = perm.mesh_index; k < perm.mesh_index + perm.mesh_count; k++)
                    {
                        var temp3 = new TreeViewItemChModel();
                        temp3.Header = k.ToString();
                        temp3.SetValue(ItemHelper.ParentProperty, temp2);
                        temp3.Value = _nodes[k];
                        temp2.Children.Add(temp3);
                    }

                    temp2.SetValue(ItemHelper.ParentProperty, temp);
                    temp.Children.Add(temp2);


                }
                
                
                _regions.Add(temp);
            };
        }

        private void ShowAllNodes()
        {
            foreach (var node in Traverse(_nodes))
                node.IsVisible = true;
        }

        private void HideAllNodes()
        {
            foreach (var node in Traverse(_nodes))
                node.IsVisible = false;
        }

        private void HideLODNodes()
        {
            foreach (var node in Traverse(_nodes))
            {
                if (_meshIdentifierService.IsLod(node.Name))
                    node.IsVisible = false;
            }
        }

        private void HideVolumeNodes()
        {
            foreach (var node in Traverse(_nodes))
            {
                if (_meshIdentifierService.IsVolume(node.Name))
                    node.IsVisible = false;
            }
        }

        private void ExpandAllNodes()
        {
            foreach (var node in Traverse(_nodes))
                node.IsExpanded = true;
        }

        private void CollapseAllNodes()
        {
            foreach (var node in Traverse(_nodes))
                node.IsExpanded = false;
        }

        private void SearchTermChanged(string searchTerm)
        {
            _searchTerm = searchTerm;
            App.Current.Dispatcher.Invoke(_nodeCollectionView.Refresh);
        }

        private void ToggleShowWireframe()
        {
            var show = ShowWireframe;
            foreach (var node in Traverse(_nodes))
                node.ShowWireframe = show;
        }

        private void ToggleShowTextures()
        {
            var show = ShowTextures;
            foreach (var node in Traverse(_nodes))
                node.ShowTexture = show;
        }

        private IEnumerable<ModelNodeModel> Traverse(IEnumerable<ModelNodeModel> rootElems)
        {
            foreach (var elem in rootElems)
            {
                yield return elem;
                foreach (var child in Traverse(elem.Items))
                    yield return child;
            }
        }

        private void UpdateMeshInfo()
        {
            var meshCount = 0;
            var vertCount = 0;
            var faceCount = 0;

            foreach (var node in Traverse(_nodes).Select(x => x.Node).OfType<MeshNode>())
            {
                if (!node.Visible)
                    continue;

                meshCount++;
                vertCount += node.Geometry.Positions.Count;
                faceCount += node.Geometry.Indices.Count / 3;
            }

            MeshCount = meshCount;
            VertexCount = vertCount;
            FaceCount = faceCount;
        }

        private void CalculateMoveSpeed(HelixToolkitScene scene)
        {
            double maxW = 0, maxH = 0, maxD = 0;
            foreach (var node in scene.Root.Traverse())
            {
                var mn = node as MeshNode;
                if (mn is null)
                    continue;

                maxW = Math.Max(maxW, mn.Bounds.Width);
                maxH = Math.Max(maxH, mn.Bounds.Height);
                maxD = Math.Max(maxD, mn.Bounds.Depth);
            }

            const double BASELINE_MAX_DIM = 2.31;
            const double BASELINE_MIN_SPEED = 0.0001;
            const double BASELINE_DEFAULT_SPEED = 0.001;
            const double BASELINE_MAX_SPEED = 1;

            var maxDim = Math.Max(maxW, Math.Max(maxH, maxD));
            var coef = maxDim / BASELINE_MAX_DIM;

            MinMoveSpeed = BASELINE_MIN_SPEED * coef;
            MoveSpeed = BASELINE_DEFAULT_SPEED * coef;
            MaxMoveSpeed = BASELINE_MAX_SPEED * coef;
        }

        private async Task ExportModel()
        {
            var result = await ShowViewModal<ModelExportOptionsView>();
            if (!(result is Tuple<ModelExportOptionsModel, TextureExportOptionsModel> options))
                return;

            var modelOptions = options.Item1;
            var textureOptions = options.Item2;

            var exportProcess = new ExportModelProcess(_file, _assimpScene, _renderModelDef, modelOptions, textureOptions, _nodes);
            await RunProcess(exportProcess);
        }

        #endregion

    }

    public class ModelNodeModel : ObservableObject
    {

        #region Data Members

        private static readonly MaterialCore DEFAULT_MATERIAL
          = new DiffuseMaterial();

        private SceneNode _node;
        private MaterialCore _material;

        #endregion

        #region Properties

        public SceneNode Node => _node;
        public string Name => _node.Name;
        public ICollection<ModelNodeModel> Items { get; }

        public bool IsExpanded { get; set; }
        [OnChangedMethod(nameof(OnNodeVisibilityChanged))]
        public bool IsVisible { get; set; }
        [OnChangedMethod(nameof(OnShowTextureChanged))]
        public bool ShowTexture { get; set; }
        [OnChangedMethod(nameof(OnShowWireframeChanged))]
        public bool ShowWireframe { get; set; }

        #endregion

        #region Constructor

        public ModelNodeModel(SceneNode node)
        {
            _node = node;
            if (node is MeshNode meshNode)
                _material = meshNode.Material;

            node.Tag = this;
            Items = new ObservableCollection<ModelNodeModel>();

            IsExpanded = true;
            IsVisible = true;

            ShowTexture = true;
            ShowWireframe = false;
        }

        #endregion

        #region Event Handlers

        private void OnShowTextureChanged()
        {
            if (_node is MeshNode meshNode)
            {
                if (ShowTexture)
                    meshNode.Material = _material;
                else
                    meshNode.Material = DEFAULT_MATERIAL;
            }
        }

        private void OnShowWireframeChanged()
        {
            if (_node is MeshNode meshNode)
                meshNode.RenderWireframe = ShowWireframe;
        }

        private void OnNodeVisibilityChanged()
        {
            _node.Visible = IsVisible;
            changeNodeNameBy(IsVisible,ref _node);
            foreach (var childNode in _node.Traverse())
            {
                var nodeModel = (childNode.Tag as ModelNodeModel);
                if (nodeModel is null)
                    continue;

                nodeModel.IsVisible = IsVisible;
                var node_ch = nodeModel.Node;
                changeNodeNameBy(IsVisible, ref node_ch);
            }
        }

        private void changeNodeNameBy(bool IsVisible, ref SceneNode node) {
            return;
            if (IsVisible)
                node.Name = node.Name.Replace("_IsVisible", "");
            else {
                node.Name = node.Name + "_IsVisible";
            }
        
        }

        #endregion

    }
}
