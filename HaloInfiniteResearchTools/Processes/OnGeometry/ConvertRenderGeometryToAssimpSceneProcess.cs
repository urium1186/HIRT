using Assimp;
using HaloInfiniteResearchTools.Assimport;
using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Common.Extensions;
using HaloInfiniteResearchTools.Processes;
using LibHIRT.Data;
using LibHIRT.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace LibHIRT.Processes.OnGeometry
{

    public class ConvertRenderGeometryToAssimpSceneProcess : ProcessBase<Scene>
    {

        #region Data Members



        private RenderGeometrySceneContext _context;
        private readonly RenderGeometry _renderGeometry;
        private readonly string _prefixMeshName;
        private readonly object _statusLock;
        private Scene _parentScene;
        private Assimp.Matrix4x4 _initialTransform;

        #endregion

        #region Properties

        //public override Scene Result => new Scene();
        public override Scene Result => _context.Scene;
        public RenderGeometry RenderGeometry => _context.Tpl;

        public CancellationToken GetCancelToken => CancellationToken;

        internal RenderGeometrySceneContext Context { get => _context; set => _context = value; }
        #endregion

        #region Constructor

        public ConvertRenderGeometryToAssimpSceneProcess(RenderGeometry renderGeometry, string prefixMeshName, Scene parentScene = null, Assimp.Matrix4x4 initialTransform = default)
        {
            _renderGeometry = renderGeometry;
            _prefixMeshName = prefixMeshName;
            _statusLock = new object();
            _parentScene = parentScene;
            _initialTransform = initialTransform;
        }

        #endregion

        #region Overrides

        protected override async Task OnExecuting()
        {
            _context = new RenderGeometrySceneContext(_renderGeometry, StatusList, _parentScene);

            ConvertObjects();
            /*
      var armatureNode = _context.Scene.RootNode.FindNode( "h" );
      if ( armatureNode != null )
        armatureNode.Name = Path.GetFileNameWithoutExtension( _file.Name );*/
        }

        #endregion

        #region Private Methods

        private void ConvertObjects()
        {
            try
            {
                var rootNode = new Node(_prefixMeshName);
                if (_context.Scene.RootNode != null)
                {

                }
                else
                {
                    _context.Scene.RootNode = _context.RootNode = rootNode;
                }

                AddMeshNodes(_context.Tpl.Meshes);
            }
            catch (Exception ex)
            {
                StatusList.AddError(_prefixMeshName, "Encountered an error attempting to convert model.", ex);
            }
        }

        private void BuildSkinCompounds()
        {/*
            Status = "Building Skin Compounds";
            IsIndeterminate = true;

            var skinCompoundIds = _context.GeometryGraph.SubMeshes
              .Select(x => x.BufferInfo.SkinCompoundId)
              .Where(x => x >= 0)
              .Distinct()
              .ToArray();

            UnitName = "Skin Compounds Built";
            CompletedUnits = 0;
            TotalUnits = skinCompoundIds.Length;
            IsIndeterminate = false;

            foreach (var skinCompoundId in skinCompoundIds)
            {
                var skinCompoundObject = _context.GeometryGraph.Objects[skinCompoundId];
                if (!skinCompoundObject.SubMeshes.Any())
                    continue;

                var builder = new MeshBuilder(_context, skinCompoundObject, skinCompoundObject.SubMeshes.First());
                builder.Build();

                _context.SkinCompounds[skinCompoundId] = builder;

                CompletedUnits++;
            }*/
        }

        private void AddNodes(List<S3DObject> objects)
        {
            Status = "Initializing Nodes";
            UnitName = "Nodes Initialized";
            CompletedUnits = 0;
            TotalUnits = objects.Count;

            var rootNode = new Node(_prefixMeshName);
            _context.Scene.RootNode = _context.RootNode = rootNode;
            rootNode.Transform = _initialTransform;
            foreach (var obj in objects)
            {
                var path = obj.UnkName;
                if (string.IsNullOrEmpty(path))
                    continue;

                var pathParts = path.Split('|', StringSplitOptions.RemoveEmptyEntries);

                var parentNode = rootNode;
                foreach (var part in pathParts)
                {
                    if (!_context.NodeNames.TryGetValue(part, out var newNode))
                    {
                        newNode = new Node(part, parentNode);
                        parentNode.Children.Add(newNode);
                        _context.NodeNames.Add(part, newNode);
                    }

                    parentNode = newNode;
                }

                var nodeName = pathParts.Last();
                var node = _context.NodeNames[nodeName];
                _context.Nodes.Add(obj.Id, node);

                node.Transform = obj.MatrixModel.ToAssimp();

                CompletedUnits++;
            }
        }

        private void AddMeshNodes(List<s_mesh> objects)
        {
            Status = "Building Meshes";
            IsIndeterminate = true;
            UnitName = "Meshes Built";


            var submeshCount = objects.Count;

            CompletedUnits = 0;
            TotalUnits = submeshCount;
            IsIndeterminate = false;

            foreach (var obj in objects)
            {
                /*
                if (_context.SkinCompounds.ContainsKey(obj.Id))
                    continue;
                */
                AddSubMeshes(obj);
            }
        }

        private void AddSubMeshes(s_mesh obj)
        {
            var node = new Node($"{obj.Name}_{obj.Name}", _context.RootNode);
            _context.RootNode.Children.Add(node);

            //foreach (var submesh in obj.SubMeshes)
            //{
            var builder = new MeshBuilder(_context, obj, null, null);
            var mesh = builder.Build();
            //var mesh = MeshBuilderNew.CreateCubeMesh(50, mesh_.Name);

            lock (_context)
            {
                _context.Scene.Meshes.Add(mesh);
                var meshId = _context.Scene.Meshes.Count - 1;
                node.MeshIndices.Add(meshId);

            }

            CompletedUnits++;
            //}
        }

        private void AddRemainingMeshBones()
        {
            // Blender sometimes freaks out if bones in the hierarchy chain aren't on the meshes.
            // Hence this icky looking method.

            var boneLookup = new Dictionary<string, Bone>();
            foreach (var mesh in _context.Scene.Meshes)
            {
                foreach (var bone in mesh.Bones)
                    if (!boneLookup.ContainsKey(bone.Name))
                        boneLookup.Add(bone.Name, new Bone { Name = bone.Name, OffsetMatrix = bone.OffsetMatrix });
            }

            foreach (var mesh in _context.Scene.Meshes)
            {
                foreach (var meshBone in mesh.Bones.ToList())
                {
                    var meshBoneNode = _context.Scene.RootNode.FindNode(meshBone.Name);
                    if (meshBoneNode is null)
                        continue;

                    var parent = meshBoneNode.Parent;
                    while (parent != null && !parent.HasMeshes)
                    {
                        if (!mesh.Bones.Any(x => x.Name == parent.Name))
                            if (boneLookup.TryGetValue(parent.Name, out var parentBone))
                                mesh.Bones.Add(new Bone { Name = parentBone.Name, OffsetMatrix = parentBone.OffsetMatrix });

                        parent = parent.Parent;
                    }
                }
            }
        }

        #endregion


    }


}
