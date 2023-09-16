using Assimp;
using HaloInfiniteResearchTools.Common;
using LibHIRT.Domain;
using System.Collections.Generic;

namespace HaloInfiniteResearchTools.Assimport
{
    internal class RenderGeometrySceneContext : ISceneContext
    {
        public StatusList StatusList { get; }
        public Node RootNode { get; set; }
        public Scene Scene { get; set; }
        private RenderGeometry tpl;
        private StatusList statusList;

        public Dictionary<short, Node> Nodes { get; }
        public Dictionary<string, Node> NodeNames { get; }

        public Dictionary<short, MeshBuilder> SkinCompounds { get; }
        public RenderGeometry Tpl { get => tpl; set => tpl = value; }

        public RenderGeometrySceneContext(RenderGeometry tpl, StatusList statusList, Scene _parentScene = null)
        {
            this.tpl = tpl;
            this.statusList = statusList;
            NodeNames = new Dictionary<string, Node>();
            Nodes = new Dictionary<short, Node>();
            StatusList = statusList;
            if (_parentScene != null)
            {
                Scene = _parentScene;
                RootNode = _parentScene.RootNode;
            }

            else
            {
                Scene = new Scene();
                Scene.Materials.Add(new Material() { Name = "DefaultMaterial" });
            }

        }
    }
}
