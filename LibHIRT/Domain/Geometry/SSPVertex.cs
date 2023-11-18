using Aspose.ThreeD;
using GlmSharp;
using System.Numerics;

namespace LibHIRT.Domain.Geometry
{
    public class GeometryNodeIndex
    {
        byte[] node_index = new byte[8] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue };

        public byte[] Node_index { get => node_index; set => node_index = value; }
    }
    public class GeometryNodeWeight
    {
        float[] node_weight = new float[8] { float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue };

        public float[] Node_weight { get => node_weight; set => node_weight = value; }
    }

    public abstract class SSPVertex : SSPGeometryElement
    {
        private readonly GeometryNodeIndex node_indices = new GeometryNodeIndex();
        private readonly GeometryNodeWeight node_weights = new GeometryNodeWeight();

        #region Properties



        public float X
        {
            get => Position.X;
        }

        public float Y
        {
            get => Position.Y;
        }

        public float Z
        {
            get => Position.Z;
        }

        public Vector3 Position { get; set; }

        public Vector2? Texcoord { get; set; }

        public Vector3? Normal { get; set; }

        public Vector3? Binormal { get; set; }

        public Vector3? Tangent { get; set; }

        public Vector2? LightmapTexcoord { get; set; }

        public GeometryNodeIndex Node_indices { get => node_indices; }
        public GeometryNodeWeight Node_weights { get => node_weights; }

        public Vector3? Vertex_color { get; set; }
        public Vector2? Texcoord1 { get; set; }
        public float Dual_quat_weight { get; set; }
        public float Vertex_alpha { get; set; }
        public Vector2? Tangent_UV2 { get; set; }
        public Vector2? Texcoord2 { get; set; }

        public Vector3? Tangent_UV3 { get; set; }

        public int FixBone { get; set; }
        public int? Index { get; set; }
        public int? MatIndex { get; set; }

        public override SSPGeometryElementType ElementType
        {
            get => SSPGeometryElementType.Vertex;
        }

        #endregion

    }

    public class SSPVertexStatic : SSPVertex
    {
    }

    public class SSPVertexSkinned : SSPVertex
    {

        #region Properties

        // Are these types correct?
        public float? Weight1 { get; set; }
        public float? Weight2 { get; set; }
        public float? Weight3 { get; set; }
        public float? Weight4 { get; set; }

        public byte Index1 { get; set; }
        public byte Index2 { get; set; }
        public byte Index3 { get; set; }
        public byte Index4 { get; set; }

        #endregion

    }

}
