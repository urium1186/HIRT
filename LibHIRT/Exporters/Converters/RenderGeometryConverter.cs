using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Shading;
using LibHIRT.Domain;
using LibHIRT.Domain.Geometry;
using LibHIRT.Serializers;
using System.Diagnostics;

namespace LibHIRT.Exporters.Converters
{
    public class RenderGeometryConverter
    {
        private RenderGeometry _renderGeometry;
        private List<Material> _materials;

        public RenderGeometryConverter(RenderGeometry renderGeometry, List<Material> materials)
        {
            _renderGeometry = renderGeometry;
            _materials = materials;
        }

        public static Mesh BuildMesh(string name, s_mesh objMesh, int lod, out List<int> mat_indexs)
        {
            Mesh temp = new Mesh(name);

            AddVertexInfo(temp, objMesh.LODRenderData[lod].Vertexs);

            mat_indexs = AddFace(temp, objMesh.IndexBufferType, objMesh.LODRenderData[lod].IndexBufferIndex, objMesh.LODRenderData[lod].Parts);
            return temp;
        }

        public Node BuildFullEntity()
        {
            Node temp = new Node("root");

            for (int i = 0; i < _renderGeometry.Meshes.Count; i++)
            {
                Node temp_n = temp.CreateChildNode(BuildMesh("mesh_" + i.ToString(), _renderGeometry.Meshes[i], 0, out var mat_indexs));
                temp_n.Materials.Clear();
                foreach (var item in mat_indexs)
                {
                    if (item >= 0 && item < _materials.Count)
                        temp_n.Materials.Add(_materials[item]);
                    else
                    {

                    }
                }


            }
            return temp;
        }

        private void AddMaterialsToMesh(Mesh m)
        {
            VertexElementMaterial vertexElementMaterial = (VertexElementMaterial)m.GetElement(VertexElementType.Material);

        }

        protected static void AddVertexInfo(Mesh mesh, SSPVertex[] vertexs)
        {
            VertexElementUV elementUV0 = mesh.CreateElementUV(TextureMapping.Diffuse);
            VertexElementUV elementUV1 = null;
            VertexElementUV elementUV2 = null;

            if (vertexs[0].Texcoord1 != null)
                elementUV1 = mesh.CreateElementUV(TextureMapping.Ambient);
            if (vertexs[0].Texcoord2 != null)
                elementUV2 = mesh.CreateElementUV(TextureMapping.Shadow);

            foreach (var item in vertexs)
            {
                mesh.ControlPoints.Add(new Aspose.ThreeD.Utilities.Vector4(item.X, item.Y, item.Z));
                elementUV0.Data.Add(new Aspose.ThreeD.Utilities.Vector4(item.Texcoord.Value.X, item.Texcoord.Value.Y, 0));
                if (elementUV1 != null)
                    elementUV1.Data.Add(new Aspose.ThreeD.Utilities.Vector4(item.Texcoord1.Value.X, item.Texcoord1.Value.Y, 0));
                if (elementUV2 != null)
                    elementUV2.Data.Add(new Aspose.ThreeD.Utilities.Vector4(item.Texcoord2.Value.X, item.Texcoord2.Value.Y, 0));

            }
           
        }

        protected static List<int> AddFace(Mesh mesh, IndexBufferType indexBufferType, List<uint> indices, s_part[] parts)
        {
            VertexElementMaterial _vertexElementMaterial = (VertexElementMaterial)mesh.CreateElement(VertexElementType.Material);
            _vertexElementMaterial.MappingMode = MappingMode.Polygon;

            List<int> material_list = new List<int>();
            /*int mat_index = 0;
            if (material_list.Contains((int)item.MatIndex))
                mat_index = material_list.IndexOf((int)item.MatIndex);
            else
            {
                material_list.Add((int)item.MatIndex);
                mat_index = material_list.Count;
            }*/

            switch (indexBufferType)
            {
                case IndexBufferType.DEFAULT:
                    break;
                case IndexBufferType.line_list:
                    break;
                case IndexBufferType.line_strip:
                    break;
                case IndexBufferType.triangle_list:
                    if (indices.Count == 0)
                    {
                        Debug.Assert(mesh.ControlPoints.Count % 3 == 0);
                        for (int i = 0; i < mesh.ControlPoints.Count; i += 3)
                        {
                            mesh.CreatePolygon(i + 0, i + 1, i + 2);
                            int mat_ind = RenderGeometrySerializer.GetMaterialIndexByFaceIndex(parts, i);
                            int mat_index = 0;
                            if (material_list.Contains(mat_ind))
                                mat_index = material_list.IndexOf(mat_ind);
                            else
                            {
                                material_list.Add(mat_ind);
                                mat_index = material_list.Count-1;
                            }
                            _vertexElementMaterial.Indices.Add(mat_index);
                            
                            
                        }

                    }
                    else
                    {
                        Debug.Assert(indices.Count % 3 == 0);
                        for (int i = 0; i < indices.Count; i += 3)
                        {
                            int cp_0 = (int)indices[i + 0];
                            int cp_1 = (int)indices[i + 1];
                            int cp_2 = (int)indices[i + 2];
                            mesh.CreatePolygon(cp_0, cp_1, cp_2);
                            int mat_ind = RenderGeometrySerializer.GetMaterialIndexByFaceIndex(parts, i);
                            int mat_index = 0;
                            if (material_list.Contains(mat_ind))
                                mat_index = material_list.IndexOf(mat_ind);
                            else
                            {
                                material_list.Add(mat_ind);
                                mat_index = material_list.Count-1;
                            }
                            _vertexElementMaterial.Indices.Add(mat_index);
                        }
                    }
                    break;
                case IndexBufferType.triangle_patch:
                    break;
                case IndexBufferType.triangle_strip:
                    break;
                case IndexBufferType.quad_list:
                    break;
                default:
                    break;
            }
            return material_list;
        }

    }
}
