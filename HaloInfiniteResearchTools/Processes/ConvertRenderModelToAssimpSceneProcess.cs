using Assimp;
using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.Common.Extensions;
using HaloInfiniteResearchTools.Processes;
using HaloInfiniteResearchTools.Processes.Utils;
using LibHIRT.Common;
using LibHIRT.Data;
using LibHIRT.Domain;
using LibHIRT.Domain.RenderModel;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.Serializers;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace LibHIRT.Processes
{

    public class ConvertRenderModelToAssimpSceneProcess : ProcessBase<Scene>
    {

        #region Data Members

        private readonly ISSpaceFile _file;

        private SceneContext _context;
        private readonly object _statusLock;
        private Scene _parentScene;
        private Assimp.Matrix4x4 _initialTransform;
        private Node skl_nodes;

        private Dictionary<string, Node> _marker_lookup;

        #endregion

        #region Properties

        //public override Scene Result => new Scene();
        public override Scene Result => _context.Scene;
        public RenderModelDefinition RenderModelDef => _context.Tpl;

        public CancellationToken GetCancelToken => CancellationToken;

        public Dictionary<string, Node> Marker_lookup { get => _marker_lookup; set => _marker_lookup = value; }

        #endregion

        #region Constructor

        public ConvertRenderModelToAssimpSceneProcess(ISSpaceFile file, Scene parentScene = null, Assimp.Matrix4x4 initialTransform = default)
        {
            _file = file;
            _statusLock = new object();
            _parentScene = parentScene;
            _initialTransform = initialTransform;
        }

        #endregion

        #region Overrides

        protected override async Task OnExecuting()
        {
            _context = await DeserializeFile();

            ConvertObjects();
            /*
      var armatureNode = _context.Scene.RootNode.FindNode( "h" );
      if ( armatureNode != null )
        armatureNode.Name = Path.GetFileNameWithoutExtension( _file.Name );*/
        }

        #endregion

        #region Private Methods

        private async Task<SceneContext> DeserializeFile()
        {
            Status = "Deserializing File";
            IsIndeterminate = true;

            var stream = _file.GetStream();

            try
            {
                stream.AcquireLock();

                if (_file is RenderModelFile)
                {
                    var tpl = RenderModelSerializer.Deserialize(stream, (RenderModelFile)_file);
                    var context = new SceneContext(tpl, stream, StatusList, _parentScene);
                    //context.AddLodDefinitions(tpl.LodDefinitions);

                    return context;
                    //return null;
                }
                else
                    throw new InvalidDataException("File must be TPL or LG.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally { stream.ReleaseLock(); }
        }

        private void ConvertObjects()
        {
            try
            {
                var rootNode = new Node(Path.GetFileNameWithoutExtension(_file.Name));
                if (_context.Scene.RootNode != null)
                {

                }
                else
                {
                    _context.Scene.RootNode = _context.RootNode = rootNode;
                }
                AddSklNodes(_context.Tpl.Nodes, _context.Tpl.TagInstance["nodes"] as ListTagInstance);
                AddGroupMarkers(_context.Tpl.Nodes, _context.Tpl.Marker_groups, _context.Tpl.TagInstance["marker groups"] as ListTagInstance);

                AddMeshNodes(_context.Tpl.Render_geometry.Meshes);
            }
            catch (Exception ex)
            {
                StatusList.AddError(_file.Name, "Encountered an error attempting to convert model.", ex);
            }
        }

        private void AddGroupMarkers(ModelBone[] nodes1, RenderModelMarkerGroup[] marker_groups, ListTagInstance? listTagInstance)
        {
            Status = "Initializing MarkerGroups";
            UnitName = "MarkerGroup Initialized";
            CompletedUnits = 0;
            TotalUnits = marker_groups.Length;
            _marker_lookup = new Dictionary<string, Node>();
            foreach (var group in marker_groups)
            {
                foreach (var marker in group.Markers)
                {
                    if (!_context.NodeNames.ContainsKey(nodes1[marker.NodeIndex].Name))
                    {
                        continue;
                    }
                    var parent = _context.NodeNames[nodes1[marker.NodeIndex].Name];
                    var marker_node = new Node($"{group.Name}_{marker.Index}_{marker.RegionIndex}_{marker.PermutationIndex}_{marker.NodeIndex}", parent);
                    marker_node.Transform = NumericExtensions.TRS(marker.Translation, marker.Rotation, marker.Scale).ToAssimp();
                    parent.Children.Add(marker_node);
                    _marker_lookup[marker_node.Name] = marker_node;
                    var mesh_1 = MeshBuilder.CreatePiramide(0.01f, $"{marker_node.Name}_marker");

                    lock (_context)
                    {
                        _context.Scene.Meshes.Add(mesh_1);
                        var meshId = _context.Scene.Meshes.Count - 1;
                        marker_node.MeshIndices.Add(meshId);

                    }
                }
            }

        }

        //new method to create a 3D cube mesh from a entry length










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

        private void AddSklNodes(ModelBone[] nodes1, ListTagInstance nodes)
        {
            Status = "Initializing Nodes";
            UnitName = "Nodes Initialized";
            CompletedUnits = 0;
            TotalUnits = nodes.Count;
            skl_nodes = new Node("skl_nodes", _context.Scene.RootNode);
            _context.RootNode.Children.Add(skl_nodes);
            AddSklNodesRecursive(nodes1, nodes, nodes1[0], skl_nodes);
        }

        private void AddSklNodesRecursive(ModelBone[] nodes1, ListTagInstance nodes, ModelBone rootBone, Node parentNode)
        {

            int parent_index = rootBone.ParentIndex;
            int first_child_node_index = rootBone.FirstChildIndex;
            int next_sibling_node_index = rootBone.NextSiblingIndex;
            var obj = nodes[rootBone.Index];
            if (parent_index != -1)
            {
                string parent_name = (nodes[parent_index]["name"] as Mmr3Hash)?.Str_value;
                if (!_context.NodeNames.TryGetValue(parent_name, out parentNode))
                {
                    return;
                }
            }
            string node_name = (obj["name"] as Mmr3Hash)?.Str_value;
            int node_id = (obj["name"] as Mmr3Hash).Value;

            Node newNode = null;

            if (!_context.NodeNames.ContainsKey(node_name))
            {
                newNode = new Node(node_name, parentNode);
                newNode.Metadata["isBoneMesh"] = new Metadata.Entry(MetaDataType.Bool, true);
                parentNode.Children.Add(newNode);
                _context.NodeNames.Add(node_name, newNode);

            }




            var node = _context.NodeNames[node_name];
            //var node = new Node(node_name, _context.Scene.RootNode);
            //_context.Scene.RootNode.Children.Add(node);
            if (!rootBone.LoadedLocalTransform)
                ModelBone.calculateLocalTransformation(rootBone);
            if (!rootBone.LoadedGlobalTransform)
                ModelBone.calculateGlobalTransformation(rootBone);

            node.Transform = rootBone.LocalTransform.ToAssimp();

            if (node.calculateGlobalTransformation().Equals(rootBone.GlobalTransform))
            {

            };
            //foreach (var submesh in obj.SubMeshes)
            //{
            var mesh = MeshBuilder.CreateCubeMesh(0.01f, $"node_mesh_{node_name}");

            lock (_context)
            {
                _context.Scene.Meshes.Add(mesh);
                var meshId = _context.Scene.Meshes.Count - 1;
                node.MeshIndices.Add(meshId);

            }

            /*
            var node1 = new Node("p_"+node_name, skl_nodes);
            skl_nodes.Children.Add(node1);
            node1.Transform = rootBone.GlobalTransform.ToAssimp();
            
            
            var mesh_1 = MeshBuilderNew.CreatePiramide(0.01f, $"node_mesh_p_{node_name}");

            lock (_context)
            {
                _context.Scene.Meshes.Add(mesh_1);
                var meshId = _context.Scene.Meshes.Count - 1;
                node1.MeshIndices.Add(meshId);

            }
            */

            CompletedUnits++;
            if (next_sibling_node_index != -1)
                AddSklNodesRecursive(nodes1, nodes, nodes1[next_sibling_node_index], parentNode);
            if (first_child_node_index != -1)
                AddSklNodesRecursive(nodes1, nodes, nodes1[first_child_node_index], node);


        }


        /*Matrix3D GetMatrix3D() {
            Matrix3D mtr = Matrix3D.Identity;   

            return mtr;
        }*/

        private void AddNodes(List<S3DObject> objects)
        {
            Status = "Initializing Nodes";
            UnitName = "Nodes Initialized";
            CompletedUnits = 0;
            TotalUnits = objects.Count;

            var rootNode = new Node(Path.GetFileNameWithoutExtension(_file.Name));
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

        private void AddMeshNodes(List<ObjMesh> objects)
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

        private void AddSubMeshes(ObjMesh obj)
        {
            var node = new Node($"{obj.Name}_{obj.Name}", _context.RootNode);
            _context.RootNode.Children.Add(node);

            //foreach (var submesh in obj.SubMeshes)
            //{
            var builder = new MeshBuilder(_context, obj, null);
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
        private void AddSklNodesOld(ModelBone[] nodes1, ListTagInstance nodes)
        {
            Status = "Initializing Nodes";
            UnitName = "Nodes Initialized";
            CompletedUnits = 0;
            TotalUnits = nodes.Count;

            var rootNode = new Node("skl_nodes", _context.Scene.RootNode);
            _context.RootNode.Children.Add(rootNode);
            //rootNode.Transform = _initialTransform;
            int i = 0;
            foreach (var obj in nodes)
            {
                var parentNode = rootNode;
                int parent_index = nodes1[i].ParentIndex;
                int first_child_node_index = nodes1[i].FirstChildIndex;
                int next_sibling_node_index = nodes1[i].NextSiblingIndex;
                if (parent_index != -1)
                {
                    string parent_name = (nodes[parent_index]["name"] as Mmr3Hash)?.Str_value;
                    if (!_context.NodeNames.TryGetValue(parent_name, out parentNode))
                    {
                        continue;
                    }
                }
                string node_name = (obj["name"] as Mmr3Hash)?.Str_value;
                int node_id = (obj["name"] as Mmr3Hash).Value;

                Node newNode = null;

                if (!_context.NodeNames.ContainsKey(node_name))
                {
                    newNode = new Node(node_name, parentNode);
                    parentNode.Children.Add(newNode);
                    _context.NodeNames.Add(node_name, newNode);
                }




                var node = _context.NodeNames[node_name];
                ModelBone.calculateLocalTransformation(nodes1[i]);
                node.Transform = nodes1[i].LocalTransform.ToAssimp();
                //foreach (var submesh in obj.SubMeshes)
                //{
                var mesh = MeshBuilder.CreateCubeMesh(0.01f, $"node_mesh_{node_name}");

                lock (_context)
                {
                    _context.Scene.Meshes.Add(mesh);
                    var meshId = _context.Scene.Meshes.Count - 1;
                    node.MeshIndices.Add(meshId);

                }
                //}
                //node.Transform. = obj.MatrixModel.ToAssimp();

                CompletedUnits++;
                i++;
            }
        }


    }


}
