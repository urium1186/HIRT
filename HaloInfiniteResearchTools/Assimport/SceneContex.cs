using Assimp;
using HaloInfiniteResearchTools.Common;
using LibHIRT.Domain.RenderModel;
using LibHIRT.Files;
using System.Collections.Generic;

namespace HaloInfiniteResearchTools.Assimport
{
    public interface ISceneContext
    {
        public Scene Scene { get; }
    }

    public class SceneContext : ISceneContext
    {
        public StatusList StatusList { get; }
        public Node RootNode { get; set; }
        public Scene Scene { get; set; }
        private RenderModelDefinition tpl;
        private HIRTStream stream;
        private StatusList statusList;

        public Dictionary<short, Node> Nodes { get; }
        public Dictionary<string, Node> NodeNames { get; }

        public Dictionary<short, MeshBuilder> SkinCompounds { get; }
        public RenderModelDefinition Tpl { get => tpl; set => tpl = value; }

        public SceneContext(RenderModelDefinition tpl, HIRTStream stream, StatusList statusList, Scene _parentScene = null)
        {
            this.tpl = tpl;
            this.stream = stream;
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
