using HaloInfiniteResearchTools.Assimport;
using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Controls;
using HaloInfiniteResearchTools.Models;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.Processes.OnGeometry;
using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.Services.Abstract;
using HaloInfiniteResearchTools.ViewModels;
using HaloInfiniteResearchTools.ViewModels.Abstract;
using HaloInfiniteResearchTools.Views;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using LibHIRT.Domain.RenderModel;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.Serializers;
using LibHIRT.TagReader;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using PhongMaterialCore = HelixToolkit.SharpDX.Core.Model.PhongMaterialCore;
using TextureModel = HelixToolkit.SharpDX.Core.TextureModel;

namespace HaloInfiniteResearchTools.ControlsModel
{



    public class Model3DViewerControlModel : ViewModel, IDisposeWithView
    {

        #region Data Members

        private readonly IHIFileContext _fileContext;
        private readonly IMeshIdentifierService _meshIdentifierService;

        private SSpaceFile _file;

        private string _searchTerm;
        private ObservableCollection<ModelNodeModel> _nodes;
        private Dictionary<string, ModelNodeModel> _nodesNameLookUp;
        private ICollectionView _nodeCollectionView;

        private HISceneContext _context;
        private RenderModelDefinition _renderModelDef;
        private Dictionary<string, Assimp.Node> _marker_lookup;
        private Dictionary<string, TextureModel> _loadedTextures;
        private RenderGeometryTag _renderGeometryTag;
        private ObservableCollection<TreeViewItemModel> _regions;
        private ObservableCollection<TreeViewItemModel> _variants;
        private ObservableCollection<TreeViewItemModel> _themeConfigurations;
        private HelixToolkitScene _sceneHelixToolkit;
        private List<(SSpaceFile, RenderModelDefinition, Assimp.Scene)> _secundaryMesh;
        private Assimp.Scene _assimpScene;

        public ISceneContext MyGeometrySceneContex { get; set; }

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
        public ICommand ExtExportModelCommand { get; set; }
        public ObservableCollection<TreeViewItemModel> Regions { get => _regions; set => _regions = value; }
        public ModelInfoToRM ModelInfo { get; set; }
        public ListTagInstance ThemeConfigurations { get; set; }
        public ObservableCollection<TreeViewItemModel> Variants { get => _variants; set => _variants = value; }
        public ObservableCollection<TreeViewItemModel> ThemeConfigurationsTVIM
        {
            get => _themeConfigurations;
            set => _themeConfigurations = value;
        }
        public RenderGeometryTag RenderGeometryTag { get => _renderGeometryTag; set => _renderGeometryTag = value; }

        #endregion

        #region Constructor

        public Model3DViewerControlModel(IServiceProvider serviceProvider, SSpaceFile file)
          : base(serviceProvider)
        {
            _file = file;
            _loadedTextures = new Dictionary<string, TextureModel>();


            _secundaryMesh = new List<(SSpaceFile, RenderModelDefinition, Assimp.Scene)>();

            _fileContext = ServiceProvider.GetRequiredService<IHIFileContext>();
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

            ShowAllCommand = new Command(ShowAllNodes);
            HideAllCommand = new Command(HideAllNodes);
            HideLODsCommand = new Command(HideLODNodes);
            HideVolumesCommand = new Command(HideVolumeNodes);
            ExpandAllCommand = new Command(ExpandAllNodes);
            CollapseAllCommand = new Command(CollapseAllNodes);
            SearchTermChangedCommand = new Command<string>(SearchTermChanged);

            ExportModelCommand = new AsyncCommand(ExportModel);

            PropertyChanged += Model3DViewerControlModel_PropertyChanged;

        }

        private void Model3DViewerControlModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {

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

            if (RenderGeometryTag == null)
                return;

            _context = new HISceneContext(_file.Name);

            ProcessBase convertProcess = null;

            if (_file is ScenarioStructureBspFile)
            {
                convertProcess = new LoadSbpsToContextProcess(_context, _file as ScenarioStructureBspFile, _file.FileMemDescriptor.GlobalTagId1.ToString("X"));
            }
            else
            {
                var renderGeometry = RenderGeometrySerializer.Deserialize(null, _file, RenderGeometryTag);
                convertProcess = new AddRenderGeometryToContextProcess(_context, renderGeometry, _file.FileMemDescriptor.GlobalTagId1.ToString("X"));
            }


            //var convertProcess = new ConvertRenderGeometryToAssimpSceneProcess(renderGeometry, _file.FileMemDescriptor.GlobalTagId1.ToString("X"));




            await Task.Factory.StartNew(convertProcess.Execute, TaskCreationOptions.LongRunning);
            await convertProcess.CompletionTask;

            _assimpScene = _file is ScenarioStructureBspFile ? ((LoadSbpsToContextProcess)convertProcess).Result : ((AddRenderGeometryToContextProcess)convertProcess).Result;

            await PrepareModelViewer(_assimpScene);

            //await RunProcess(convertProcess);



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
                MyGeometrySceneContex = new RenderGeometrySceneContext(null, null);


                Viewport.Items.Clear();
                Viewport.Items.Add(Model);
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

        public async Task ExportModel(Tuple<ModelExportOptionsModel, TextureExportOptionsModel> result)
        {
            if (!(result is Tuple<ModelExportOptionsModel, TextureExportOptionsModel> options))
                return;

            var modelOptions = options.Item1;
            var textureOptions = options.Item2;
            ExportModelProcess exportProcess;
            if (_assimpScene != null)
                exportProcess = new ExportModelProcess(_file, _assimpScene, _renderModelDef, modelOptions, textureOptions, _nodes);
            else
                exportProcess = new ExportModelProcess(_file, _sceneHelixToolkit, _renderModelDef, modelOptions, textureOptions, _nodes);
            await RunProcess(exportProcess);

            for (int i = 0; i < _secundaryMesh.Count; i++)
            {
                var temp = _secundaryMesh[i];
                var exportProcessN = new ExportModelProcess(temp.Item1, temp.Item3, temp.Item2, modelOptions, textureOptions, _nodes);
                await RunProcess(exportProcessN);

            }
        }
        private async Task ExportModel()
        {
            try
            {
                if (ExtExportModelCommand != null)
                {
                    ExtExportModelCommand.Execute(this);
                    return;
                }

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
            catch (Exception ex)
            {

                throw ex;
            }




        }

        #endregion

    }

}
