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

namespace LibHIRT.Exporters.Converters
{
    public class SbspConverter
    {
        private Dictionary<int, Dictionary<int, int>> uniqueInstanceMesh = new Dictionary<int, Dictionary<int, int>>();
        private ScenarioStructureBspFile _scenarioStructure;
        private Scene _scene;

        public SbspConverter(ScenarioStructureBspFile scenarioStructure, Scene scene) {
            _scenarioStructure= scenarioStructure;
            _scene= scene;
        }

        void AddMeshByClusters(TagInstance root, Node nodeRoot)
        {
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
                        AddPerInstanceData(tagBlock, instance_index, nodeRoot, rootRtgo, renderGeometry, rtgo_file);
                    }
                }

            }
        }

        void AddPerInstanceData(ListTagInstance tagBlock, int i, Node nodeRoot, TagInstance rootRtgo, LibHIRT.Domain.RenderGeometry renderGeometry, SSpaceFile rtgo_file)
        {
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
            /*if (!uniqueInstanceMesh[rtgo_file.FileMemDescriptor.GlobalTagId1].ContainsKey(meshIndex))
            {
                temp = _context.AddRenderGeometry(_prefixMeshName + rtgo_file.Name + "_" + i, renderGeometry, null, new List<int> { meshIndex }, true);
                uniqueInstanceMesh[rtgo_file.FileMemDescriptor.GlobalTagId1][meshIndex] = temp.MeshIndices[0];
            }
            else
            {
                temp = new Node(_prefixMeshName + rtgo_file.Name + "_" + i);
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
            nodeRoot.Children.Add(temp);*/
        }

    }


}
