using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Common.Extensions;
using HaloInfiniteResearchTools.Common.Grunt;
using HaloInfiniteResearchTools.Controls;
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
using LibHIRT.Common;
using LibHIRT.Domain.RenderModel;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.Processes.OnGeometry;
using LibHIRT.TagReader;
using Microsoft.Extensions.DependencyInjection;
using OpenSpartan.Grunt.Models.HaloInfinite;
using PropertyChanged;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using PhongMaterialCore = HelixToolkit.SharpDX.Core.Model.PhongMaterialCore;
using TextureModel = HelixToolkit.SharpDX.Core.TextureModel;

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
        private Dictionary<string, ModelNodeModel> _nodesNameLookUp;
        private ICollectionView _nodeCollectionView;

        private Assimp.Scene _assimpScene;
        private RenderModelDefinition _renderModelDef;
        private Dictionary<string, Assimp.Node> _marker_lookup;
        private Dictionary<string, TextureModel> _loadedTextures;
        private ObservableCollection<TreeViewItemModel> _regions;
        private ObservableCollection<TreeViewItemModel> _variants;
        private ObservableCollection<TreeViewItemModel> _themeConfigurations;
        private HelixToolkitScene _sceneHelixToolkit;
        private List<(SSpaceFile, RenderModelDefinition, Assimp.Scene)> _secundaryMesh;



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
        public bool HaveCoreInfo { get; set; }

        public ICollectionView Nodes => _nodeCollectionView;



        public int MeshCount { get; set; }
        public int VertexCount { get; set; }
        public int FaceCount { get; set; }

        public List<ArmorCore> CoresInfo { get; set; }

        private Dictionary<string, string> CmsJsonPair;
        private Dictionary<int, TreeViewItemModel> attachments_dict = new Dictionary<int, TreeViewItemModel>();

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
        public ObservableCollection<TreeViewItemModel> ThemeConfigurationsTVIM
        {
            get => _themeConfigurations;
            set => _themeConfigurations = value;
        }

        #endregion

        #region Constructor

        public RenderModelViewModel(IServiceProvider serviceProvider, SSpaceFile file)
          : base(serviceProvider)
        {
            _file = file;
            _loadedTextures = new Dictionary<string, TextureModel>();

            _secundaryMesh = new List<(SSpaceFile, RenderModelDefinition, Assimp.Scene)>();

            _fileContext = HIFileContext.Instance;
            _meshIdentifierService = ServiceProvider.GetRequiredService<IMeshIdentifierService>();

            EffectsManager = new DefaultEffectsManager();
            Camera = new PerspectiveCamera() { FarPlaneDistance = 300000 };
            Model = new SceneNodeGroupModel3D();
            ApplyTransformsToModel();

            _nodes = new ObservableCollection<ModelNodeModel>();
            _nodeCollectionView = InitializeNodeCollectionView(_nodes);

            _nodesNameLookUp = new Dictionary<string, ModelNodeModel>();

            _regions = new ObservableCollection<TreeViewItemModel>();

            _variants = new ObservableCollection<TreeViewItemModel>();
            _themeConfigurations = new ObservableCollection<TreeViewItemModel>();

            ShowTextures = true;
            HaveCoreInfo = false;

            ShowAllCommand = new Command(ShowAllNodes);
            HideAllCommand = new Command(HideAllNodes);
            HideLODsCommand = new Command(HideLODNodes);
            HideVolumesCommand = new Command(HideVolumeNodes);
            ExpandAllCommand = new Command(ExpandAllNodes);
            CollapseAllCommand = new Command(CollapseAllNodes);
            SearchTermChangedCommand = new Command<string>(SearchTermChanged);

            ExportModelCommand = new AsyncCommand(ExportModel);

        }

        private void RenderModelViewModel_ChangeNodeAttacth(object? sender, ICheckedModel e)
        {

        }

        #endregion

        #region Overrides

        protected override async Task OnInitializing()
        {
            Options = GetPreferences().ModelViewerOptions;
            UseFlycam = Options.DefaultToFlycam;

            await LoadCoresAsync();
            var convertProcess = new ConvertRenderModelToAssimpSceneProcess(_file);
            await RunProcess(convertProcess);

            _assimpScene = convertProcess.Result;


            _renderModelDef = convertProcess.RenderModelDef;
            _marker_lookup = convertProcess.Marker_lookup;


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

            var file = _fileContext.GetFiles<PictureFile>(name).ElementAt(0);
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
              new System.Windows.Media.Media3D.Vector3D(1, 0, 0), -90);

            transformGroup.Children.Add(rotTransform);

            var rotTransform2 = new System.Windows.Media.Media3D.RotateTransform3D();
            rotTransform2.Rotation = new System.Windows.Media.Media3D.AxisAngleRotation3D(
              new System.Windows.Media.Media3D.Vector3D(0, 1, 0), -90);
            transformGroup.Children.Add(rotTransform2);


            var scaleTransform = new System.Windows.Media.Media3D.ScaleTransform3D(1, 1, 1);
            transformGroup.Children.Add(scaleTransform);

            Model.Transform = transformGroup;
        }

        private Matrix getGlobalTransformation()
        {
            var transformGroup = new System.Windows.Media.Media3D.Transform3DGroup();

            var rotTransform = new System.Windows.Media.Media3D.RotateTransform3D();
            rotTransform.Rotation = new System.Windows.Media.Media3D.AxisAngleRotation3D(
              new System.Windows.Media.Media3D.Vector3D(1, 0, 0), -90);

            transformGroup.Children.Add(rotTransform);
            var rotTransform2 = new System.Windows.Media.Media3D.RotateTransform3D();
            rotTransform2.Rotation = new System.Windows.Media.Media3D.AxisAngleRotation3D(
              new System.Windows.Media.Media3D.Vector3D(0, 1, 0), -90);
            transformGroup.Children.Add(rotTransform2);


            var scaleTransform = new System.Windows.Media.Media3D.ScaleTransform3D(1, 1, 1);
            //transformGroup.Children.Add(scaleTransform);

            return transformGroup.ToMatrix();
        }

        private async Task PrepareModelViewer(Assimp.Scene assimpScene)
        {
            //await ReadMeshAttachmentsModelsAsync(assimpScene);
            var importer = new Importer();
            importer.ToHelixToolkitScene(assimpScene, out var scene);


            _sceneHelixToolkit = scene;

            AddNodeModels(scene.Root);
            FillRegionsList();
            FillVariantsList();
            await FillThemeConfigurationListAsync(assimpScene);

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

            await Task.Delay(450).ContinueWith(t =>
            {
                App.Current.Dispatcher.Invoke(() => Camera.ZoomExtents(Viewport));
            });
        }
        void AddNodeModels(SceneNode node)
        {
            if (node is MeshNode meshNode)
            {
                meshNode.CullMode = SharpDX.Direct3D11.CullMode.Back;

                var nodeModel = new ModelNodeModel(node);
                nodeModel.NodeVisibilityChanged += NodeModel_NodeVisibilityChanged;
                _nodes.Add(nodeModel);
                _nodesNameLookUp[node.Name] = nodeModel;
            }
            var i = 0;
            foreach (var childNode in node.Items)
            {
                AddNodeModels(childNode);
                i++;
            }

        }

        private async void NodeModel_NodeVisibilityChanged(object? sender, ModelNodeModel e)
        {

            var temp = e.Node.Tag as ParNodes;
            if (temp == null)
                return;
            if (_nodesNameLookUp.TryGetValue(temp.Marker.Name + "_marker", out var value))
            {
                var m_n = value.Node as MeshNode;
                if (m_n == null)
                    return;
                var a_n = e.Node as MeshNode;
                if (a_n == null)
                    return;
                if (m_n.Tag == null)
                    m_n.Tag = m_n.Geometry;

                if (e.IsVisible)
                {
                    m_n.Geometry = a_n.Geometry;
                    m_n.Visible = true;
                    a_n.Visible = false;
                    _assimpScene.Meshes[temp.Marker.MeshIndices[0]] = temp.Attach.Meshes[1];
                }
                else
                {
                    if (m_n.Tag != null && m_n.Tag is Geometry3D)
                        m_n.Geometry = (Geometry3D)m_n.Tag;
                    _assimpScene.Meshes[temp.Marker.MeshIndices[0]] = new Assimp.Mesh();
                    m_n.Visible = false;
                    a_n.Visible = false;
                }


            }
        }

        private void FillVariantsList()
        {
            if (ModelInfo.Variants != null)
            {
                foreach (var item in ModelInfo.Variants)
                {

                    var style = (item["style"] as Mmr3Hash).Str_value;
                    var runtimeVariantRegionIndices = (item["runtime variant region indices"] as ArrayFixLen);

                    var damageStyleIndex = (item["Damage Style Index"] as FourByte);
                    TreeViewItemModel temp1 = GetMeshInVariant(item, _renderModelDef, ModelInfo);
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

        TreeViewItemChModel getMeshIndexByRegionPermName(int regionNameId, int permuNameId, RenderModelDefinition renderModelDef, ref TreeViewItemModel parent)
        {
            foreach (var region in renderModelDef.Regions)
            {
                if (region.name_id == regionNameId)
                {
                    foreach (var perm in region.permutations)
                    {
                        if (perm.name_id == permuNameId)
                        {
                            for (int k = perm.mesh_index; k < perm.mesh_index + perm.mesh_count; k++)
                            {
                                var temp3 = new TreeViewItemChModel();
                                temp3.Header = k.ToString();
                                temp3.SetValue(ItemHelper.ParentProperty, parent);
                                string lod_name = renderModelDef.Render_geometry.Meshes[k].Name;
                                //$"{lod_name}_{lod_name}"
                                temp3.Value = _nodesNameLookUp[lod_name];
                                parent.Children.Add(temp3);
                            }
                        }
                    }
                }
            }
            return null;
        }
        private async Task FillThemeConfigurationListAsync(Assimp.Scene assimpScene)
        {
            if (ThemeConfigurations == null)
                return;

            foreach (var themeConfiguration in ThemeConfigurations)
            {
                var temp = new TreeViewItemModel();
                temp.Header = themeConfiguration["Theme Name"].AccessValue.ToString();
                var result = HIFileContext.Instance.GetFileFrom(themeConfiguration["Theme Configs"] as TagRef, _file.Parent as ModuleFile);
                if (result != null)
                {
                    var r = result as ObjectCustomizationThemeConfiguration;
                    #region Regions

                    ReadMeshCoustoms(r, ref temp, "Regions");

                    #endregion
                    #region Attachments

                    await ReadMeshAttachmentsAsync(r, temp, assimpScene);

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
        private async Task ReadMeshAttachmentsModelsAsync(Assimp.Scene assimpScene)
        {
            if (ThemeConfigurations == null)
                return;

            foreach (var themeConfiguration in ThemeConfigurations)
            {

                var result = HIFileContext.Instance.GetFileFrom(themeConfiguration["Theme Configs"] as TagRef, _file.Parent as ModuleFile);
                if (result != null)
                {
                    var r = result as ObjectCustomizationThemeConfiguration;


                    var items = r.Deserialized?["Attachments"] as ListTagInstance;
                    foreach (var item in items)
                    {

                        TagRef tagRef = item["Attachment"] as TagRef;
                        if (tagRef == null || tagRef.Ref_id_int == -1)
                            continue;
                        CustomizationAttachmentConfiguration attachFile = HIFileContext.Instance.GetFileFrom(tagRef) as CustomizationAttachmentConfiguration;
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
                            ModelFile modelFile = HIFileContext.Instance.GetFileFrom(attach_model_ref) as ModelFile;
                            if (modelFile == null)
                                continue;
                            var rmf = modelFile.GetRenderModel();
                            if (rmf == null)
                                continue;
                            // Assimp.Matrix4x4 initialTransform = GetTransformationMatrixFrom((ListTagInstance)(modelAttachment["Markers"] ),0,0);

                            var convertProcess = new ConvertRenderModelToAssimpSceneProcess(rmf);

                            RenderModelDefinition temp_renderModelDef = null;
                            try
                            {
                                await RunProcess(convertProcess);

                                temp_renderModelDef = convertProcess.RenderModelDef;

                                //assimpScene.RootNode.Children.AddRange(convertProcess.Result.RootNode.Children.ToArray());
                                var importer = new Importer();
                                importer.ToHelixToolkitScene(convertProcess.Result, out var scene);



                                AddNodeModels(scene.Root);
                                Model.AddNode(scene.Root);
                            }
                            catch (Exception exi)
                            {
                                temp_renderModelDef = convertProcess.RenderModelDef;

                            }
                        }
                    }
                }
            }
        }

        private Assimp.Matrix4x4 GetTransformationMatrixFrom(ListTagInstance listTagInstance, int region_index, int permutation_index)
        {
            var tempRMD = this._renderModelDef.TagInstance;
            ListTagInstance listTag = tempRMD["marker groups"] as ListTagInstance;
            if (listTag == null)
                return default;

            Assimp.Matrix4x4 result = default;
            if (listTagInstance == null || listTagInstance.Count == 0)
                return result;
            var temp = listTagInstance[0];
            TagInstance marketInfo = null;
            foreach (var item in listTag)
            {
                if ((item["name"] as Mmr3Hash).Value == (temp["Marker Name"] as Mmr3Hash).Value)
                {
                    foreach (var itemMarker in item["markers"] as ListTagInstance)
                    {
                        if ((sbyte)itemMarker["region index"].AccessValue == region_index && (Int32)itemMarker["permutation index"].AccessValue == permutation_index)
                        {
                            marketInfo = itemMarker;
                            break;
                        }
                    }
                    break;
                }
            }
            if (marketInfo == null)
                return default;
            return result;
        }

        private void SetAtachmentToRegions(Assimp.Scene attachnodeMesh, TagInstance attachmentDef, TreeViewItemModel meshs, TreeViewItemModel regions, int tag_id)
        {
            var tempRMD = this._renderModelDef.TagInstance;
            var markerGroup = this._renderModelDef.Marker_groups;
            ListTagInstance marker_groups = tempRMD["marker groups"] as ListTagInstance;
            if (marker_groups == null)
                return;

            Assimp.Matrix4x4 result = default;
            var listTagInstance = attachmentDef["Markers"] as ListTagInstance;
            if (listTagInstance == null || listTagInstance.Count == 0)
                return;
            var marketInfo = listTagInstance[0];
            Debug.Assert(listTagInstance.Count == 1);

            var a_trs = marketInfo["Translation"] as Point3D;
            var a_rts = marketInfo["Rotation"] as Point3D;
            var a_scl = marketInfo["Scale"] as Point3D;
            // get vector3 from a_tras, a_rts, a_scl
            Vector3 at_trs = new Vector3
            {
                X = a_trs.X,
                Y = a_trs.Y,
                Z = a_trs.Z
            };
            Vector3 at_rts = new Vector3
            {
                X = a_rts.X,
                Y = a_rts.Y,
                Z = a_rts.Z
            };
            Vector3 at_scl = new Vector3
            {
                X = a_scl.X,
                Y = a_scl.Y,
                Z = a_scl.Z
            };
            TagRef attach_model_ref = attachmentDef["Attachment Model"] as TagRef;
            string attachModelName = $"M_I:{attach_model_ref.Ref_id}"; // _Var:{(attachmentDef["Variant"] as Mmr3Hash).Str_value}

            int m_g_index = 0;
            foreach (var marker_group in marker_groups)
            {
                m_g_index = m_g_index + 1;
                string name = (marker_group["name"] as Mmr3Hash).Str_value;
                if ((marker_group["name"] as Mmr3Hash).Value == (marketInfo["Marker Name"] as Mmr3Hash).Value)
                {

                    var markers = marker_group["markers"] as ListTagInstance;
                    if (markers == null)
                        continue;
                    int i = 0;
                    foreach (var itemMarker in markers)
                    {
                        int r_i = (sbyte)itemMarker["region index"].AccessValue;
                        int p_i = (Int32)itemMarker["permutation index"].AccessValue;
                        int node_index = (Int16)itemMarker["node index"].AccessValue;
                        var direc = markerGroup[m_g_index - 1].Markers[i].Direction.getVectorDirection();
                        var position = markerGroup[m_g_index - 1].Markers[i].Direction.getVectorDirection(_renderModelDef.Nodes[node_index].LocalTransform);
                        i++;
                        if (r_i == -1 || p_i == -1)
                            continue;

                        //if ( r_i < regions.Children.Count )
                        if (r_i < _renderModelDef.Regions.Length)
                        {


                            //var top = ((regions.Children[r_i] as TreeViewItemModel).Children[0]) as TreeViewItemModel;
                            if (!(p_i < _renderModelDef.Regions[r_i].permutations.Length))
                                continue;

                            TreeViewItemModel tvm = getVariant(regions, _renderModelDef.Regions[r_i].name_id, _renderModelDef.Regions[r_i].permutations[p_i].name_id, false);





                            if (tvm == null)
                                continue;
                            CheckedModel copy_tvm = (CheckedModel)tvm.copy();

                            if (tvm.Children.Count == 0 || tvm.Children[0].Header != "Attachments")
                            {
                                TreeViewItemModel tempTVIMs = new TreeViewItemModel();
                                tempTVIMs.Header = "Attachments";
                                tvm.Children.Insert(0, tempTVIMs);
                                tempTVIMs.SetValue(ItemHelper.ParentProperty, tvm);
                            }
                            var tvmAttachments = (tvm.Children[0] as TreeViewItemModel);


                            TreeViewItemModel tempTVIM = new TreeViewItemModel();
                            tempTVIM.Header = $"{attachModelName}_MG-{m_g_index - 1}_M-{i - 1}_In_R-{r_i}_P-{p_i}_N-{node_index}";
                            _marker_lookup.TryGetValue($"{name}_{i - 1}_{r_i}_{p_i}_{node_index}", out var nodeMarker);

                            tempTVIM.Tag = new ParNodes
                            {
                                Attach = attachnodeMesh,
                                Marker = nodeMarker,
                            };

                            var meshs_copy = meshs.copy() as TreeViewItemModel;
                            meshs_copy.SetValue(ItemHelper.ParentProperty, tempTVIM);
                            tempTVIM.Children.Add(meshs_copy);
                            attachments_dict[tag_id] = meshs_copy;

                            tempTVIM.SetValue(ItemHelper.ParentProperty, tvmAttachments);
                            tvmAttachments.Children.Add(tempTVIM);
                        }
                    }
                    break;
                }
            }

        }

        SharpDX.Matrix GetTransformMatrixFrom(TagInstance rd_maker, TagInstance marker)
        {
            SharpDX.Matrix.Scaling(2, 2, 2);
            var trs = rd_maker["translation"] as Point3D;
            var rts = rd_maker["rotation"] as LibHIRT.TagReader.Quaternion;
            float scl = (rd_maker["scale"] as Float).Value;
            var dir = rd_maker["direction"] as Point3D;
            int node_index = (Int16)rd_maker["node index"].AccessValue;

            var m_trs = marker["Translation"] as Point3D;
            var m_rts = marker["Rotation"] as Point3D;
            var m_scl = marker["Scale"] as Point3D;
            var node = (_renderModelDef.TagInstance["nodes"] as ListTagInstance)[node_index];

            SharpDX.Matrix result = SharpDX.Matrix.Identity;
            SharpDX.Matrix rotM = SharpDX.Matrix.Identity;
            SharpDX.Quaternion rot = new SharpDX.Quaternion
            {
                X = rts.X,
                Y = rts.Y,
                Z = rts.Z,
                W = rts.W
            };
            SharpDX.Matrix.RotationQuaternion(ref rot, out rotM);


            var translV = new SharpDX.Vector3
            {
                X = -trs.X,
                Y = trs.Z,
                Z = trs.Y
            };
            //result = result * rotM;
            var scaleV = new SharpDX.Vector3
            {
                X = 1,
                Y = 1,
                Z = 1
            };
            var tempLocalM = NumericExtensions.TRS(new System.Numerics.Vector3
            {
                X = -trs.X,
                Y = trs.Z,
                Z = trs.Y
            }, new System.Numerics.Quaternion
            {
                X = rts.X,
                Y = rts.Y,
                Z = rts.Z,
                W = rts.W
            }, new System.Numerics.Vector3
            {
                X = 1,
                Y = 1,
                Z = 1
            });
            result = SharpDX.Matrix.Transformation(new SharpDX.Vector3(0), new SharpDX.Quaternion(0), scaleV, new SharpDX.Vector3(), rot, translV);
            ModelBone.calculateGlobalTransformation(_renderModelDef.Nodes[node_index]);
            result = (_renderModelDef.Nodes[node_index].GlobalTransform).ToSharpDX(false);
            result.Transpose();
            if (_nodesNameLookUp.TryGetValue("node_mesh_" + _renderModelDef.Nodes[node_index].Name, out var nodeModel))
            {
                var t_v = (nodeModel.Node as MeshNode).Parent.ModelMatrix;
                result = (nodeModel.Node as MeshNode).Parent.TotalModelMatrix;
                if (!result.IsIdentity)
                {
                }
            }
            //tempLocalM *
            //result = result * getGlobalTransformation()  ;
            return result;
        }

        TreeViewItemModel getVariant(TreeViewItemModel regions, int r_i, int p_i, bool v)
        {
            foreach (var regionTrv in regions.Children)
            {
                TreeViewItemModel childs = regionTrv as TreeViewItemModel;
                foreach (var subregionTrv in childs.Children)
                {
                    if (subregionTrv.Header == r_i.ToString())
                    {
                        foreach (var itemVar in (subregionTrv as TreeViewItemModel).Children)
                        {
                            if (itemVar.Header == p_i.ToString())
                            {
                                if (!v)
                                    return (TreeViewItemModel)itemVar;
                                else
                                {
                                    return (TreeViewItemModel)itemVar.copy();

                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private async Task ReadMeshAttachmentsAsync(ObjectCustomizationThemeConfiguration? r, TreeViewItemModel parent, Assimp.Scene assimpScene)
        {
            TreeViewItemModel tempPart = new TreeViewItemModel();
            tempPart.Header = "Attachments";
            Dictionary<string, TreeViewItemModel> keyValuePairs = new Dictionary<string, TreeViewItemModel>();
            var items = r.Deserialized?[tempPart.Header] as ListTagInstance;
            foreach (var item in items)
            {

                TagRef tagRef = item["Attachment"] as TagRef;
                if (tagRef == null || tagRef.Ref_id_int == -1)
                    continue;
                CustomizationAttachmentConfiguration attachFile = HIFileContext.Instance.GetFileFrom(tagRef) as CustomizationAttachmentConfiguration;
                if (attachFile == null)
                    continue;
                ListTagInstance tgl = attachFile.Deserialized.Root["Model Attachments"] as ListTagInstance;
                if (tgl == null || tgl.Count == 0)
                    continue;

                foreach (var modelAttachment in tgl)
                {
                    TagRef attach_model_ref = modelAttachment["Attachment Model"] as TagRef;
                    EnumGroup att_csm_type = modelAttachment["CMS Customization Item Type"] as EnumGroup;


                    if (!keyValuePairs.ContainsKey(att_csm_type.Selected))
                    {
                        keyValuePairs[att_csm_type.Selected] = new TreeViewItemModel();
                        keyValuePairs[att_csm_type.Selected].Header = att_csm_type.Selected;
                        keyValuePairs[att_csm_type.Selected].SetValue(ItemHelper.ParentProperty, tempPart);
                        tempPart.Children.Add(keyValuePairs[att_csm_type.Selected]);
                    }



                    if (attach_model_ref == null || attach_model_ref.TagGroupRev != "hlmt")
                        continue;
                    ModelFile modelFile = HIFileContext.Instance.GetFileFrom(attach_model_ref) as ModelFile;
                    if (modelFile == null)
                        continue;
                    var rmf = modelFile.GetRenderModel();
                    if (rmf == null)
                        continue;
                    var convertProcess = new ConvertRenderModelToAssimpSceneProcess(rmf);

                    RenderModelDefinition temp_renderModelDef = null;
                    try
                    {
                        await RunProcess(convertProcess);

                        temp_renderModelDef = convertProcess.RenderModelDef;

                        //assimpScene.RootNode.Children.AddRange(convertProcess.Result.RootNode.Children.ToArray());
                        var importer = new Importer();
                        importer.ToHelixToolkitScene(convertProcess.Result, out var scene);


                        _secundaryMesh.Add((modelFile, temp_renderModelDef, convertProcess.Result));
                        AddNodeModels(scene.Root);
                        Model.AddNode(scene.Root);
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
                        if ((item_variant["name"] as Mmr3Hash).AccessValue.ToString() == modelAttachment["Variant"].AccessValue.ToString())
                        {
                            found = true;
                            temp_tvm = GetMeshInVariant(item_variant, temp_renderModelDef, vara_reg);


                            break;
                        }

                    }
                    if (!found)
                        continue;
                    SetAtachmentToRegions(convertProcess.Result, modelAttachment, temp_tvm, (TreeViewItemModel)parent.Children[0], tagRef.Ref_id_int);
                    TreeViewItemModel temp1 = new TreeViewItemModel
                    {
                        Header = modelAttachment["Variant"].AccessValue.ToString()
                    };

                    temp_tvm.SetValue(ItemHelper.ParentProperty, keyValuePairs[att_csm_type.Selected]);

                    keyValuePairs[att_csm_type.Selected].Children.Add(temp_tvm);
                }
            }
            tempPart.SetValue(ItemHelper.ParentProperty, parent);
            parent.Children.Add(tempPart);
        }
        private List<TreeViewItemChModel> GetPermutationBy(int regName, int perName)
        {
            for (int i = 0; i < _renderModelDef.Regions.Length; i++)
            {
                if (_renderModelDef.Regions[i].name_id == regName)
                {
                    for (int j = 0; j < _renderModelDef.Regions[i].permutations.Length; j++)
                    {
                        if (_renderModelDef.Regions[i].permutations[j].name_id == perName)
                        {
                            var perm = _renderModelDef.Regions[i].permutations[j];
                            List<TreeViewItemChModel> result = new List<TreeViewItemChModel>();
                            for (int k = perm.mesh_index; k < perm.mesh_index + perm.mesh_count; k++)
                            {
                                var temp = new TreeViewItemChModel();
                                temp.Header = k.ToString();
                                //marketInfo.Value = _nodes[k];
                                //$"{obj.Name}_{mesh.Name}";
                                string k_str = _renderModelDef.Render_geometry.Meshes[k].Name;
                                temp.Value = _nodesNameLookUp[k_str];
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
                        temp3.Header = _renderModelDef.Render_geometry.Meshes[k].Name;
                        temp3.SetValue(ItemHelper.ParentProperty, temp2);
                        temp3.Value = _nodesNameLookUp[temp3.Header];
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
            //var exportProcess = new ExportModelProcess(_file, _sceneHelixToolkit, _renderModelDef, modelOptions, textureOptions, _nodes);
            await RunProcess(exportProcess);

            for (int i = 0; i < _secundaryMesh.Count; i++)
            {
                var temp = _secundaryMesh[i];
                var exportProcessN = new ExportModelProcess(temp.Item1, temp.Item3, temp.Item2, modelOptions, textureOptions, _nodes);
                await RunProcess(exportProcessN);

            }





        }

        public void SelectCore(ArmorCore armorCore)
        {
            if (this.CmsJsonPair != null)
            {
                string jsonString_temp = CmsJsonPair[armorCore.Themes[0].ThemePath];
                ArmorTheme tempArmorTheme = (ArmorTheme)JsonSerializer.Deserialize(jsonString_temp, typeof(ArmorTheme), JsonSerializerFix.SerializerOptions);

                HideAllCommand.Execute(true);

                SelectRegionsDatas(tempArmorTheme.CoreRegionData.BaseRegionData);

                SelectArmorCorePart(armorCore.Themes[0].GlovePath);
                SelectArmorCorePart(armorCore.Themes[0].HelmetPath);
                SelectArmorCorePart(armorCore.Themes[0].KneePadPath);
                SelectArmorCorePart(armorCore.Themes[0].RightShoulderPadPath);
                SelectArmorCorePart(armorCore.Themes[0].LeftShoulderPadPath);

                SelectArmorCoreAttach(armorCore.Themes[0].ChestAttachmentPath);
                SelectArmorCoreAttach(armorCore.Themes[0].HelmetAttachmentPath);
                SelectArmorCoreAttach(armorCore.Themes[0].HipAttachmentPath);
                SelectArmorCoreAttach(armorCore.Themes[0].WristAttachmentPath);
                if (CmsJsonPair.ContainsKey("bodyCustomization.json"))
                {
                    string bodyCustomization_temp = CmsJsonPair["bodyCustomization.json"];
                    BodyCustomization bodyCustomization = (BodyCustomization)JsonSerializer.Deserialize(bodyCustomization_temp, typeof(BodyCustomization), JsonSerializerFix.SerializerOptions);
                    SelectBodyCustomization(bodyCustomization, tempArmorTheme.CoreRegionData);
                }

            }
        }

        private void SelectArmorCorePart(string path)
        {
            if (CmsJsonPair.ContainsKey(path))
            {
                try
                {
                    ArmorCorePart _armorCorePart = (ArmorCorePart)JsonSerializer.Deserialize(CmsJsonPair[path], typeof(ArmorCorePart), JsonSerializerFix.SerializerOptions);
                    SelectRegionsDatas(_armorCorePart.RegionData);
                }
                catch (Exception noImop)
                {

                }
            }
        }

        private void SelectArmorCoreAttach(string path)
        {
            if (CmsJsonPair.ContainsKey(path))
            {
                try
                {
                    ArmorCoreAttach _armorCorePart = (ArmorCoreAttach)JsonSerializer.Deserialize(CmsJsonPair[path], typeof(ArmorCoreAttach), JsonSerializerFix.SerializerOptions);
                    if (attachments_dict.ContainsKey(_armorCorePart.TagId))
                    {
                        var temp = attachments_dict[_armorCorePart.TagId];
                        SelectInTreeViewItemModel(temp);
                    }
                }
                catch (Exception noImop)
                {

                }
            }
        }

        private void SelectBodyCustomization(BodyCustomization bodyCustomization, CoreRegionData coreRegionData)
        {
            if (bodyCustomization.BodyType == "Small")
            {
                SelectRegionsDatas(coreRegionData.BodyTypeSmallOverrides, false, true);
                SelectRegionsDatas(coreRegionData.BodyTypeSmallOverrides);
            }
            else if (bodyCustomization.BodyType == "Large")
            {
                SelectRegionsDatas(coreRegionData.BodyTypeLargeOverrides, false, true);
                SelectRegionsDatas(coreRegionData.BodyTypeLargeOverrides);
            }
            switch (bodyCustomization.LeftArm)
            {
                case "Full":
                    SelectRegionsDatas(coreRegionData.ProstheticLeftArmOverrides.Full, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticLeftArmOverrides.Full);
                    break;
                case "Half":
                    SelectRegionsDatas(coreRegionData.ProstheticLeftArmOverrides.Half, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticLeftArmOverrides.Half);
                    break;
                case "Extremity":
                    SelectRegionsDatas(coreRegionData.ProstheticLeftArmOverrides.Extremity, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticLeftArmOverrides.Extremity);
                    break;
                default:
                    break;
            }

            switch (bodyCustomization.RightArm)
            {
                case "Full":
                    SelectRegionsDatas(coreRegionData.ProstheticRightArmOverrides.Full, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticRightArmOverrides.Full);
                    break;
                case "Half":
                    SelectRegionsDatas(coreRegionData.ProstheticRightArmOverrides.Half, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticRightArmOverrides.Half);
                    break;
                case "Extremity":
                    SelectRegionsDatas(coreRegionData.ProstheticRightArmOverrides.Extremity, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticRightArmOverrides.Extremity);
                    break;
                default:
                    break;
            }

            switch (bodyCustomization.LeftLeg)
            {
                case "Full":
                    SelectRegionsDatas(coreRegionData.ProstheticLeftLegOverrides.Full, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticLeftLegOverrides.Full);
                    break;
                case "Half":
                    SelectRegionsDatas(coreRegionData.ProstheticLeftLegOverrides.Half, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticLeftLegOverrides.Half);
                    break;
                case "Extremity":
                    SelectRegionsDatas(coreRegionData.ProstheticLeftLegOverrides.Extremity, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticLeftLegOverrides.Extremity);
                    break;
                default:
                    break;
            }

            switch (bodyCustomization.RightLeg)
            {
                case "Full":
                    SelectRegionsDatas(coreRegionData.ProstheticRightLegOverrides.Full, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticRightLegOverrides.Full);
                    break;
                case "Half":
                    SelectRegionsDatas(coreRegionData.ProstheticRightLegOverrides.Half, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticRightLegOverrides.Half);
                    break;
                case "Extremity":
                    SelectRegionsDatas(coreRegionData.ProstheticRightLegOverrides.Extremity, false, true);
                    SelectRegionsDatas(coreRegionData.ProstheticRightLegOverrides.Extremity);
                    break;
                default:
                    break;
            }


        }
        private void SelectRegionsDatas(List<RegionData>? regionsDatas, bool value = true, bool allRegion = false)
        {
            foreach (RegionData region_data in regionsDatas)
            {
                foreach (var region_ch in _regions)
                {
                    if (region_ch.Header == region_data.RegionId.MIdentifier.ToString())
                    {
                        foreach (var permu_ch in region_ch.Children)
                        {
                            if (allRegion)
                            {
                                permu_ch.IsChecked = value;
                                if (permu_ch is TreeViewItemModel)
                                {
                                    foreach (var mesh_ch in (permu_ch as TreeViewItemModel).Children)
                                    {
                                        mesh_ch.IsChecked = value;
                                        ((mesh_ch as TreeViewItemChModel).Value as ModelNodeModel).IsVisible = value;
                                    }
                                }
                            }
                            else
                            {
                                if (permu_ch.Header == region_data.PermutationId.MIdentifier.ToString())
                                {
                                    permu_ch.IsChecked = value;
                                    if (permu_ch is TreeViewItemModel)
                                    {
                                        foreach (var mesh_ch in (permu_ch as TreeViewItemModel).Children)
                                        {
                                            mesh_ch.IsChecked = value;
                                            ((mesh_ch as TreeViewItemChModel).Value as ModelNodeModel).IsVisible = value;
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        private void SelectInTreeViewItemModel(TreeViewItemModel parent)
        {
            foreach (var mesh_ch in parent.Children)
            {
                if (mesh_ch is TreeViewItemChModel)
                {
                    //(mesh_ch as CheckedModel).IsChecked = true;
                    ItemHelper.SetIsChecked((DependencyObject)mesh_ch, true);
                    //((mesh_ch as TreeViewItemChModel).Value as ModelNodeModel).IsVisible = true;
                }
                else if (mesh_ch is TreeViewItemModel)
                {
                    SelectInTreeViewItemModel(mesh_ch as TreeViewItemModel);
                }

            }
        }

        protected async Task LoadCoresAsync()
        {
            var objLock = new object();


            try
            {
                var process = new GetArmorCoresFromJsonProcess();
                process.Completed += GetArmorCoresFromJsonProcess_Completed;
                await RunProcess(process);

            }
            catch (Exception ex)
            {

            }
            finally
            {
                lock (objLock)
                {

                }

            }
        }

        private void GetArmorCoresFromJsonProcess_Completed(object? sender, EventArgs e)
        {
            this.CoresInfo = ((GetArmorCoresFromJsonProcess)sender).Result;
            if (this.CoresInfo != null && this.CoresInfo.Count > 0)
                HaveCoreInfo = true;
            this.CmsJsonPair = ((GetArmorCoresFromJsonProcess)sender).CmsJsonPair;
        }

        #endregion

    }

}
