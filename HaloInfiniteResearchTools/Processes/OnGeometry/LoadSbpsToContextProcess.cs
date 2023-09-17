using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Assimp;
using HaloInfiniteResearchTools.Assimport;
using HaloInfiniteResearchTools.Common.Extensions;
using LibHIRT.Common;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.Serializers;
using LibHIRT.TagReader;
using OpenSpartan.Grunt.Models.HaloInfinite;

namespace HaloInfiniteResearchTools.Processes.OnGeometry
{

    public class LoadSbpsToContextProcess : ProcessBase<Scene>
    {


        #region Data Members

        private HISceneContext _context;
        private ScenarioStructureBspFile _scenarioStructure;
        private readonly string _prefixMeshName;
        private readonly object _statusLock;

        private System.Numerics.Matrix4x4 _initialTransform;

        private Dictionary<int, Dictionary<int,int>> uniqueInstanceMesh = new Dictionary<int, Dictionary<int, int>>();

        #endregion

        #region Properties

        //public override Scene Result => new Scene();
        public override Scene Result => _context.Scene;

        public CancellationToken GetCancelToken => CancellationToken;

        internal HISceneContext Context { get => _context; set => _context = value; }
        #endregion

        #region Constructor

        public LoadSbpsToContextProcess(HISceneContext context, ScenarioStructureBspFile scenarioStructure, string prefixMeshName, System.Numerics.Matrix4x4 initialTransform = default)
        {
            _scenarioStructure = scenarioStructure;
            _prefixMeshName = scenarioStructure.Name;
            _context = context;
            _statusLock = new object();
            _initialTransform = initialTransform;
        }

        #endregion

        #region Overrides

        protected override async Task OnExecuting()
        {
            var root = _scenarioStructure.Deserialized()?.Root;

            if (root == null) { return; }
            Node nodeRoot = new Node(_prefixMeshName);


            AddMeshByClusters(root, nodeRoot);
            //AddMeshByRenderInstance(root, nodeRoot);



            _context.Scene.RootNode= nodeRoot;
            /*Node temp = _context.AddRenderGeometry(_prefixMeshName, _renderGeometry);
            _context.Scene.RootNode = temp;*/
        }

        void AddMeshByRenderInstance(TagInstance root, Node nodeRoot) {
            ListTagInstance tagBlock = (ListTagInstance)root["instanced geometry instances"];
            if (tagBlock == null) { return; }
            for (int i = 0; (i < tagBlock.Count); i++) // && i<5000
            {
                DoSomething(tagBlock, i, nodeRoot);
            }
        }

        void AddMeshByClusters(TagInstance root, Node nodeRoot) {
            ListTagInstance clusters = (ListTagInstance)root["clusters"];
            if (clusters == null) { return; }
            ListTagInstance tagBlock = (ListTagInstance)root["instanced geometry instances"];
            if (tagBlock == null) { return; }
            foreach (var cluster in clusters)
            {
                ListTagInstance instanceBucketsBlock = (ListTagInstance)cluster["instance Buckets Block"];
                if (instanceBucketsBlock == null) { continue; }
                foreach (var instanceBucketBlock in instanceBucketsBlock)
                {
                    TagRef tr_rtgo = instanceBucketBlock["runtime geo tag reference"] as TagRef;
                    if (tr_rtgo == null)
                        continue;
                    int runtime_geo_mesh_index = (int)instanceBucketBlock["runtime geo mesh index"].AccessValue;
                    ListTagInstance instancesDataBlock = (ListTagInstance)instanceBucketBlock["instances Data Block"];
                    if (instancesDataBlock == null) { continue; }

                    SSpaceFile rtgo_file = (SSpaceFile)HIFileContext.GetFileFrom(tr_rtgo, _scenarioStructure.Parent as ModuleFile);
                    if (rtgo_file==null)
                        continue;

                    Debug.Assert(!uniqueInstanceMesh.ContainsKey(rtgo_file.FileMemDescriptor.GlobalTagId1));
                    uniqueInstanceMesh[rtgo_file.FileMemDescriptor.GlobalTagId1] = new Dictionary<int, int>();

                    TagInstance rootRtgo = rtgo_file.Deserialized()?.Root;

                    if (rootRtgo == null)
                        continue;

                    var renderGeometry = RenderGeometrySerializer.Deserialize(null, rtgo_file, (RenderGeometryTag)rootRtgo["render geometry"]);

                    foreach (var instanceDataBlock in instancesDataBlock)
                    {
                        int instance_index = (short)instanceDataBlock["instance index"].AccessValue;
                        AddPerInstanceData(tagBlock, instance_index, nodeRoot, rootRtgo, renderGeometry, rtgo_file);
                    }
                }

            }
        }

        void AddPerInstanceData(ListTagInstance tagBlock, int i, Node nodeRoot, TagInstance rootRtgo, LibHIRT.Domain.RenderGeometry renderGeometry, SSpaceFile rtgo_file) {
            TagRef tr_rtgo = tagBlock[i]["Runtime geo mesh reference"] as TagRef;
            if (tr_rtgo == null)
                return;
            Debug.Assert(rtgo_file.FileMemDescriptor.GlobalTagId1 == tr_rtgo.Ref_id_int);
            int unique_io_index = (short)tagBlock[i]["unique io index"].AccessValue;

            int meshIndex = (Int16)tagBlock[i]["Runtime geo mesh index"].AccessValue;

            string name = rtgo_file.Name;
            if (name.Contains("207352254_1C26EF0A"))
            {

            }
            Node temp = null;
            if (!uniqueInstanceMesh[rtgo_file.FileMemDescriptor.GlobalTagId1].ContainsKey(meshIndex))
            {
                temp = _context.AddRenderGeometry(_prefixMeshName + rtgo_file.Name + "_" + i, renderGeometry, null, new List<int> { meshIndex }, true);
                uniqueInstanceMesh[rtgo_file.FileMemDescriptor.GlobalTagId1][meshIndex] = temp.MeshIndices[0];
            }
            else {
                temp = new Node(_prefixMeshName + rtgo_file.Name+"_"+i);
                temp.MeshIndices.Add(uniqueInstanceMesh[rtgo_file.FileMemDescriptor.GlobalTagId1][meshIndex]);
            }


            var scaleTagG = (Point3D)tagBlock[i]["scale"];

            var positionTagG = (Point3D)tagBlock[i]["position"];
            GlmSharp.vec3 traslationG = default;
            traslationG.x = positionTagG.X;
            traslationG.y = positionTagG.Y;
            traslationG.z = positionTagG.Z;



            var fowardTagG = (Point3D)tagBlock[i]["forward"];
            System.Numerics.Vector3 fowardG = new System.Numerics.Vector3(fowardTagG.X, fowardTagG.Y, fowardTagG.Z);
            var leftTagG = (Point3D)tagBlock[i]["left"];
            System.Numerics.Vector3 leftG = new System.Numerics.Vector3(leftTagG.X, leftTagG.Y, leftTagG.Z);
            var upTagG = (Point3D)tagBlock[i]["up"];
            System.Numerics.Vector3 upG = new System.Numerics.Vector3(upTagG.X, upTagG.Y, upTagG.Z);


            GlmSharp.mat3 meshrot_mat_g = NumericExtensions.GetRoitationFrom(fowardG, leftG, upG);


            GlmSharp.vec3 scaleG = default;
            scaleG.x = scaleTagG.X;
            scaleG.y = scaleTagG.Y;
            scaleG.z = scaleTagG.Z;

            temp.Transform = NumericExtensions.TRS(meshrot_mat_g, traslationG, scaleG).ToAssimp();
            nodeRoot.Children.Add(temp);
        }

        void DoSomething(ListTagInstance tagBlock, int i, Node nodeRoot) {
            TagRef tr_rtgo = tagBlock[i]["Runtime geo mesh reference"] as TagRef;
            if (tr_rtgo == null)
                return;
            int unique_io_index = (short)tagBlock[i]["unique io index"].AccessValue;
            /*if (!uniqueMesh.ContainsKey(unique_io_index))
            {
                uniqueMesh[unique_io_index] = new List<int> { i };
            }
            else {
                uniqueMesh[unique_io_index].Add(i);
                continue;
            }*/
            SSpaceFile rtgo_file = (SSpaceFile)HIFileContext.GetFileFrom(tr_rtgo, _scenarioStructure.Parent as ModuleFile);
            TagInstance rootRtgo = rtgo_file.Deserialized()?.Root;
            int meshIndex = (Int16)tagBlock[i]["Runtime geo mesh index"].AccessValue;
            var renderGeometry = RenderGeometrySerializer.Deserialize(null, rtgo_file, (RenderGeometryTag)rootRtgo["render geometry"]);
            string name = rtgo_file.Name;
            if (name.Contains("207352254_1C26EF0A"))
            {

            }

            Node temp = _context.AddRenderGeometry(_prefixMeshName + rtgo_file.Name, renderGeometry, null, new List<int> { meshIndex }, true);
            ListTagInstance per_mesh_data = (rootRtgo["Per Mesh Data"] as ListTagInstance);
            /*
            if (per_mesh_data != null && per_mesh_data.Count > meshIndex) {
                var scaleTag = (Point3D)per_mesh_data[meshIndex]["Scale"];
                //var rotTag = per_mesh_data[meshIndex]["Scale"];
                var positionTag = (Point3D)per_mesh_data[meshIndex]["Position"];
                GlmSharp.vec3 traslation = default;
                traslation.x = positionTag.X;
                traslation.y = positionTag.Y;
                traslation.z = positionTag.Z;



                var fowardTag = (Point3D)per_mesh_data[meshIndex]["Forward"];
                System.Numerics.Vector3 foward = new System.Numerics.Vector3(fowardTag.X, fowardTag.Y, fowardTag.Z);
                var lefTag = (Point3D)per_mesh_data[meshIndex]["Left"];
                System.Numerics.Vector3 left = new System.Numerics.Vector3(lefTag.X, lefTag.Y, lefTag.Z);
                var upTag = (Point3D)per_mesh_data[meshIndex]["Up"];
                System.Numerics.Vector3 up = new System.Numerics.Vector3(upTag.X, upTag.Y, upTag.Z);


                GlmSharp.mat3 meshrot_mat = NumericExtensions.GetRoitationFrom(foward, left, up);

                GlmSharp.vec3 scale = default;
                scale.x = scaleTag.X;
                scale.y = scaleTag.Y;
                scale.z = scaleTag.Z;

                //temp.Transform = NumericExtensions.TRS(meshrot_mat, traslation, scale).ToAssimp();
            }*/

            var scaleTagG = (Point3D)tagBlock[i]["scale"];
            //var rotTag = per_mesh_data[meshIndex]["Scale"];
            var positionTagG = (Point3D)tagBlock[i]["position"];
            GlmSharp.vec3 traslationG = default;
            traslationG.x = positionTagG.X;
            traslationG.y = positionTagG.Y;
            traslationG.z = positionTagG.Z;



            var fowardTagG = (Point3D)tagBlock[i]["forward"];
            System.Numerics.Vector3 fowardG = new System.Numerics.Vector3(fowardTagG.X, fowardTagG.Y, fowardTagG.Z);
            var leftTagG = (Point3D)tagBlock[i]["left"];
            System.Numerics.Vector3 leftG = new System.Numerics.Vector3(leftTagG.X, leftTagG.Y, leftTagG.Z);
            var upTagG = (Point3D)tagBlock[i]["up"];
            System.Numerics.Vector3 upG = new System.Numerics.Vector3(upTagG.X, upTagG.Y, upTagG.Z);


            GlmSharp.mat3 meshrot_mat_g = NumericExtensions.GetRoitationFrom(fowardG, leftG, upG);
            //System.Numerics.Quaternion rotationG = default;

            GlmSharp.vec3 scaleG = default;
            scaleG.x = scaleTagG.X;
            scaleG.y = scaleTagG.Y;
            scaleG.z = scaleTagG.Z;

            temp.Transform = NumericExtensions.TRS(meshrot_mat_g, traslationG, scaleG).ToAssimp();
            //Node onBps = new Node();
            //onBps.Transform = NumericExtensions.TRS(meshrot_mat_g, traslationG, scaleG).ToAssimp();
            //onBps.Children.Add(temp);
            //nodeRoot.Children.Add(onBps);
            nodeRoot.Children.Add(temp);
        }
        #endregion


    }


}
