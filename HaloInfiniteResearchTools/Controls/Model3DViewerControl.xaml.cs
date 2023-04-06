using System.Windows.Controls;
using HaloInfiniteResearchTools.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using HaloInfiniteResearchTools.Models;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using PropertyChanged;
using MaterialCore = HelixToolkit.SharpDX.Core.Model.MaterialCore;
using SharpDX;
using System.Windows;
using HaloInfiniteResearchTools.Services.Abstract;

namespace HaloInfiniteResearchTools.Controls
{
    /// <summary>
    /// Interaction logic for Model3DViewerControl.xaml
    /// </summary>
    public partial class Model3DViewerControl : UserControl, IDisposable
    {
        private string _searchTerm;
        private ObservableCollection<ModelNodeModel> _nodes;
        private Dictionary<string, ModelNodeModel> _nodesNameLookUp;
        private ICollectionView _nodeCollectionView;

        public string StringName { get; set; }

        public ModelViewerOptionsModel Options { get; set; }

        public Camera Camera { get; set; }
        public SceneNodeGroupModel3D Model { get; }

        private readonly IMeshIdentifierService _meshIdentifierService;

        public EffectsManager EffectsManager { get; }
        public Viewport3DX Viewport { get; 
            set; }

        public double MinMoveSpeed { get; set; }
        public double MoveSpeed { get; set; }
        public double MaxMoveSpeed { get; set; }

        [OnChangedMethod(nameof(ToggleShowWireframe))]
        public bool ShowWireframe { get; set; }

        private object ToggleShowWireframe()
        {
            throw new NotImplementedException();
        }

        [OnChangedMethod(nameof(ToggleShowTextures))]
        public bool ShowTextures { get; set; }

        private object ToggleShowTextures()
        {
            throw new NotImplementedException();
        }

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

        public Model3DViewerControl()
        {
            EffectsManager = new DefaultEffectsManager();
            Camera = new PerspectiveCamera() { FarPlaneDistance = 300000 };
            StringName = "Meshes";
            InitializeComponent();
        }
        #region Private Methods

        private void UpdateColumnsWidth(ListView listView)
        {
            if (listView is null)
                return;

            var gridView = listView.View as GridView;
            if (gridView is null)
                return;

            var lastColumnIdx = gridView.Columns.Count - 1;
            if (listView.ActualWidth == double.NaN)
                listView.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var remainingSpace = listView.ActualWidth;
            for (int i = 0; i < gridView.Columns.Count; i++)
                if (i != lastColumnIdx)
                    remainingSpace -= gridView.Columns[i].ActualWidth;

            gridView.Columns[lastColumnIdx].Width = remainingSpace >= 0 ? remainingSpace : 0;
        }

        #endregion

        #region Event Handlers

        private void OnContextMenuLoaded(object sender, RoutedEventArgs e)
        {
            (sender as ContextMenu).DataContext = this.DataContext;
        }

        private void OnListViewSizeChanged(object sender, SizeChangedEventArgs e)
          => UpdateColumnsWidth(sender as ListView);

        private void OnListViewLoaded(object sender, RoutedEventArgs e)
          => UpdateColumnsWidth(sender as ListView);

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

        }

        public void Dispose()
        {
            Model?.Dispose();
            EffectsManager?.DisposeAllResources();
            GCHelper.ForceCollect();
            ModelViewer?.Dispose();

        }

        #endregion

    }

   
}
