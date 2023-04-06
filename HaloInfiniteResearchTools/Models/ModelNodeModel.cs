using HaloInfiniteResearchTools.Controls;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Model;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;

namespace HaloInfiniteResearchTools.Models
{
    public class ModelNodeModel : ObservableObject
    {
        public event EventHandler<ModelNodeModel> NodeVisibilityChanged;
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
            if (_node.Tag is ParNodes)
                NodeVisibilityChanged?.Invoke(this, this);
            changeNodeNameBy(IsVisible, ref _node);
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

        private void changeNodeNameBy(bool IsVisible, ref SceneNode node)
        {
            return;
            if (IsVisible)
                node.Name = node.Name.Replace("_IsVisible", "");
            else
            {
                node.Name = node.Name + "_IsVisible";
            }

        }

        #endregion

    }


}
