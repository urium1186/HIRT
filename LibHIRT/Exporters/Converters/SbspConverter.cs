using Aspose.ThreeD;
using LibHIRT.Common;
using LibHIRT.Files.FileTypes;
using LibHIRT.Files;
using LibHIRT.Serializers;
using LibHIRT.TagReader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.ThreeD.Shading;
using Aspose.ThreeD.Entities;
using System.Reflection;
using GlmSharp;

namespace LibHIRT.Exporters.Converters
{
    public class SbspConverter
    {
        private Dictionary<int, Dictionary<int, int>> uniqueInstanceMesh = new Dictionary<int, Dictionary<int, int>>();
        List<Mesh> intsMesh = new List<Mesh>();
        private ScenarioStructureBspFile _scenarioStructure;
        Dictionary<int,Material> _materials = new Dictionary<int, Material> ();

        private List<int> instancedId = new List<int> ();

        public SbspConverter(ScenarioStructureBspFile scenarioStructure) {
            _scenarioStructure= scenarioStructure;
        }

        public Node BuildFullEntity() {
            Node nodeRoot = new Node(_scenarioStructure.FileMemDescriptor.GlobalTagId1.ToString());

           // AddMeshInstanceOnIndex(_scenarioStructure.Deserialized()?.Root, nodeRoot, nodeRoot.Name, 7615);
            //AddMeshInstanceOnIndex(_scenarioStructure.Deserialized()?.Root, nodeRoot, nodeRoot.Name, 7617);
           AddMeshByClusters(_scenarioStructure.Deserialized()?.Root, nodeRoot, nodeRoot.Name);
            return nodeRoot;
        }

        public Node BuildFullEntity(Node nodeRoot)
        {
            AddMeshInstanceOnIndex(_scenarioStructure.Deserialized()?.Root, nodeRoot, nodeRoot.Name, 7615);
            AddMeshInstanceOnIndex(_scenarioStructure.Deserialized()?.Root, nodeRoot, nodeRoot.Name, 7617);
            //AddMeshByClusters(_scenarioStructure.Deserialized()?.Root, nodeRoot, nodeRoot.Name);
            return nodeRoot;
        }
        void AddMeshByClusters(TagInstance root, Node nodeRoot, string prefixMeshName)
        {
            if (root == null) { return; }
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
                    if (rtgo_file == null)
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
                        AddPerInstanceData(tagBlock, instance_index, prefixMeshName, nodeRoot, rootRtgo, renderGeometry, rtgo_file);
                    }
                }

            }
        }

        void AddMeshInstanceOnIndex(TagInstance root, Node nodeRoot, string prefixMeshName, int instance_index)
        {
            if (root == null) { return; }
            ListTagInstance tagBlock = (ListTagInstance)root["instanced geometry instances"];
            if (tagBlock == null) { return; }
                
            TagRef tr_rtgo = tagBlock[instance_index]["Runtime geo mesh reference"] as TagRef;
            if (tr_rtgo == null)
                return;
                    
            SSpaceFile rtgo_file = (SSpaceFile)HIFileContext.GetFileFrom(tr_rtgo, _scenarioStructure.Parent as ModuleFile);
            if (rtgo_file == null)
                return;

            Debug.Assert(!uniqueInstanceMesh.ContainsKey(rtgo_file.FileMemDescriptor.GlobalTagId1));
            uniqueInstanceMesh[rtgo_file.FileMemDescriptor.GlobalTagId1] = new Dictionary<int, int>();

            TagInstance rootRtgo = rtgo_file.Deserialized()?.Root;

            if (rootRtgo == null)
                return;

            var renderGeometry = RenderGeometrySerializer.Deserialize(null, rtgo_file, (RenderGeometryTag)rootRtgo["render geometry"]);

                    
            AddPerInstanceData(tagBlock, instance_index, prefixMeshName, nodeRoot, rootRtgo, renderGeometry, rtgo_file);
        }

        void AddPerInstanceData(ListTagInstance tagBlock, int i,string prefixMeshName, Node nodeRoot, TagInstance rootRtgo, LibHIRT.Domain.RenderGeometry renderGeometry, SSpaceFile rtgo_file)
        {
            TagRef tr_rtgo = tagBlock[i]["Runtime geo mesh reference"] as TagRef;
            if (tr_rtgo == null)
                return;
            Debug.Assert(rtgo_file.FileMemDescriptor.GlobalTagId1 == tr_rtgo.Ref_id_int);
            int unique_io_index = (short)tagBlock[i]["unique io index"].AccessValue;

            if (instancedId.Contains(i))
            {

            }
            else {
                instancedId.Add(i);
            }

            int meshIndex = (Int16)tagBlock[i]["Runtime geo mesh index"].AccessValue;
            int hlod_index = (Int16)tagBlock[i]["hlod index"].AccessValue;

            string name = rtgo_file.Name;
           
            //Node temp = new Node(prefixMeshName + rtgo_file.Name + "_" + i);
            Mesh mesh = null;
            if (!uniqueInstanceMesh[rtgo_file.FileMemDescriptor.GlobalTagId1].ContainsKey(meshIndex))
            {
                List<int> mat_indexs;
                mesh = RenderGeometryConverter.BuildMesh(rtgo_file.FileMemDescriptor.GlobalTagId1 + "_mesh_" + meshIndex, renderGeometry.Meshes[meshIndex], hlod_index, out mat_indexs);
                if (mesh != null)
                {
                    intsMesh.Add(mesh);
                }
                    //_context.AddRenderGeometry(_prefixMeshName + rtgo_file.Name + "_" + i, renderGeometry, null, new List<int> { meshIndex }, true);
                uniqueInstanceMesh[rtgo_file.FileMemDescriptor.GlobalTagId1][meshIndex] = intsMesh.Count-1;
            }
            else
            {
                mesh = (intsMesh[uniqueInstanceMesh[rtgo_file.FileMemDescriptor.GlobalTagId1][meshIndex]]);
            }
           
            Node nodeFix = nodeRoot.CreateChildNode("instance_"+ i.ToString(),mesh);
            TagInstance material_override_data = tagBlock[i]["material override data"];
            if (material_override_data != null)
            {
                ListTagInstance perInstanceMaterialBlock = material_override_data["per Instance Material Block"] as ListTagInstance;


                foreach (var item in perInstanceMaterialBlock)
                {
                    TagRef material = (item["material"] as TagRef);
                    int global_id = material == null ? -1 : material.Ref_id_int;
                    string mat_name = material == null ? "default-1" : material.Ref_id;
                    if (!_materials.ContainsKey(global_id)) {
                        PbrMaterial mat = new PbrMaterial();

                        mat.Name = mat_name;

                        // an almost metal material

                        mat.MetallicFactor = 0.9;

                        // material surface is very rough

                        mat.RoughnessFactor = 0.9;

                        _materials[global_id] = mat;
                        nodeFix.Materials.Add(mat);
                    }
                    else
                        nodeFix.Materials.Add(_materials[global_id]);
                }
            }

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
            //var quater = GlmSharp.quat.FromMat3(meshrot_mat_g);
            var scaleTagG = (Point3D)tagBlock[i]["scale"];

            GlmSharp.vec3 scaleG = new (1);
            scaleG.x = scaleTagG.X;
            scaleG.y = scaleTagG.Y;
            scaleG.z = scaleTagG.Z;

            nodeFix.Transform.TransformMatrix = NumericExtensions.TRS(meshrot_mat_g, traslationG, scaleG).From(false);
            /*
            nodeFix.Transform.Translation = new Aspose.ThreeD.Utilities.Vector3(traslationG.x, traslationG.y, traslationG.z);
            nodeFix.Transform.Rotation = new Aspose.ThreeD.Utilities.Quaternion(quater.w, quater.x, quater.y, quater.z);
            nodeFix.Transform.Scale = new Aspose.ThreeD.Utilities.Vector3(scaleG.x, scaleG.y, scaleG.z);
            //nodeFix.EvaluateGlobalTransform(false);
            */
        }

    }


}
