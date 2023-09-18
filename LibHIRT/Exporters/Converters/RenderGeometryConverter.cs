using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Shading;
using LibHIRT.Domain;
using LibHIRT.Domain.Geometry;
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

            mat_indexs = AddVertexInfo(temp, objMesh.LODRenderData[lod].Vertexs);

            AddFace(temp, objMesh.IndexBufferType, objMesh.LODRenderData[lod].IndexBufferIndex);
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

        protected static List<int> AddVertexInfo(Mesh mesh, SSPVertex[] vertexs)
        {
            VertexElementUV elementUV0 = mesh.CreateElementUV(TextureMapping.Diffuse);
            VertexElementUV elementUV1 = null;
            VertexElementUV elementUV2 = null;
            VertexElementMaterial vertexElementMaterial = (VertexElementMaterial)mesh.CreateElement(VertexElementType.Material);
            List<int> material_list = new List<int>();

            if (vertexs[0].UV1 != null)
                elementUV1 = mesh.CreateElementUV(TextureMapping.Ambient);
            if (vertexs[0].UV2 != null)
                elementUV2 = mesh.CreateElementUV(TextureMapping.Shadow);

            foreach (var item in vertexs)
            {
                mesh.ControlPoints.Add(new Aspose.ThreeD.Utilities.Vector4(item.X, item.Y, item.Z));
                elementUV0.Data.Add(new Aspose.ThreeD.Utilities.Vector4(item.UV0.Value.X, item.UV0.Value.Y, 0));
                int mat_index = 0;
                if (material_list.Contains((int)item.MatIndex))
                    mat_index = material_list.IndexOf((int)item.MatIndex);
                else
                {
                    material_list.Add((int)item.MatIndex);
                    mat_index = material_list.Count;
                }
                vertexElementMaterial.Indices.Add(mat_index);
                if (elementUV1 != null)
                    elementUV1.Data.Add(new Aspose.ThreeD.Utilities.Vector4(item.UV1.Value.X, item.UV1.Value.Y, 0));
                if (elementUV2 != null)
                    elementUV2.Data.Add(new Aspose.ThreeD.Utilities.Vector4(item.UV2.Value.X, item.UV2.Value.Y, 0));

            }
            return material_list;
        }

        protected static void AddFace(Mesh mesh, IndexBufferType indexBufferType, List<uint> indices)
        {

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
                        }

                    }
                    else
                    {
                        Debug.Assert(indices.Count % 3 == 0);
                        for (int i = 0; i < indices.Count; i += 3)
                        {
                            mesh.CreatePolygon((int)indices[i + 0], (int)indices[i + 1], (int)indices[i + 2]);
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
        }

    }
}
