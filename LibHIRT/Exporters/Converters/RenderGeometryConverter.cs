using Aspose.ThreeD.Entities;
using LibHIRT.Domain;
using LibHIRT.Domain.DX;
using LibHIRT.Domain.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Exporters.Converters
{
    public class RenderGeometryConverter
    {
        private RenderGeometry _renderGeometry;

        public RenderGeometryConverter(RenderGeometry renderGeometry)
        {
            _renderGeometry = renderGeometry;
        }

        public Mesh BuildMesh(ObjMesh objMesh, int lod) {   
            return null;
        }

        protected void AddVertexInfo(Mesh mesh, SSPVertex[] vertexs)
        {
            foreach (var item in vertexs)
            {
                mesh.ControlPoints.Add(new Aspose.ThreeD.Utilities.Vector4( item.X, item.Y, item.Z));
            }
        }

        protected void AddFace(Mesh mesh,IndexBufferType indexBufferType, int[] indices) {
            
            switch (indexBufferType)
            {
                case IndexBufferType.DEFAULT:
                    break;
                case IndexBufferType.line_list:
                    break;
                case IndexBufferType.line_strip:
                    break;
                case IndexBufferType.triangle_list:
                    if (indices.Length == 0)
                    {
                        Debug.Assert(mesh.VertexElements.Count % 3 == 0);
                        for (int i = 0; i < mesh.VertexElements.Count; i += 3)
                        {
                            mesh.CreatePolygon(i + 0, i + 1, i + 2);
                        }
                        
                    }
                    else {
                        Debug.Assert(indices.Length % 3 == 0);
                        for (int i = 0; i < indices.Length; i += 3)
                        {
                            mesh.CreatePolygon(indices[i + 0], indices[i + 1], indices[i + 2]);
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
