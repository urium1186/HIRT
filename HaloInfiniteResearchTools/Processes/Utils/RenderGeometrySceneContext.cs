﻿using Assimp;
using HaloInfiniteResearchTools.Common;
using LibHIRT.Domain;
using LibHIRT.Domain.RenderModel;
using LibHIRT.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes.Utils
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
            this.NodeNames = new Dictionary<string, Node>();
            this.Nodes = new Dictionary<short, Node>();
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
