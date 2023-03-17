using System.Numerics;

namespace LibHIRT.Domain.Geometry
{

    public abstract class SSPVertex : SSPGeometryElement
    {

        #region Properties

        public Vector4 Position { get; set; }

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

        public Vector2? UV0 { get; set; }
        public Vector2? UV1 { get; set; }
        public Vector2? UV3 { get; set; }

        public Vector4? Normal { get; set; }
        public Vector4? Tangent { get; set; }
        public Vector4? Colour { get; set; }

        public int? Index { get; set; }

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
