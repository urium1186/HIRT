using System.Collections;
using static LibHIRT.Assertions;

namespace LibHIRT.Domain.Geometry
{

    public abstract class SSPFace : SSPGeometryElement, IEnumerable<ushort>
    {

        #region Data Members

        protected ushort[] _vertexIndices;

        #endregion

        #region Properties

        public int FaceCount
        {
            get => _vertexIndices.Length;
        }

        public override SSPGeometryElementType ElementType
        {
            get => SSPGeometryElementType.Face;
        }

        public ushort this[int index]
        {
            get => _vertexIndices[index];
            set => _vertexIndices[index] = value;
        }

        #endregion

        #region Constructor

        protected SSPFace(ushort[] vertexIndices)
        {
            _vertexIndices = vertexIndices;
        }

        public static SSPFace Create(ushort[] vertexIndices)
        {
            Assert(vertexIndices.Length > 1,
              "An SSPFace must have at least 2 vertices.");

            switch (vertexIndices.Length)
            {
                case 3:
                    return new SSPFaceTri(vertexIndices);
                case 4:
                    return new SSPFaceQuad(vertexIndices);
                default:
                    return new SSPFaceNgon(vertexIndices);
            }
        }

        #endregion

        #region IEnumerable Methods

        public IEnumerator<ushort> GetEnumerator()
        {
            foreach (var index in _vertexIndices)
                yield return index;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var index in _vertexIndices)
                yield return index;
        }

        #endregion

    }

    public class SSPEdge : SSPFace
    {

        #region Properties

        public ushort VertexA
        {
            get => this[0];
            set => this[0] = value;
        }

        public ushort VertexB
        {
            get => this[1];
            set => this[1] = value;
        }

        #endregion

        #region Constructor

        public SSPEdge(ushort[] vertexIndices)
          : base(vertexIndices)
        {
            Assert(vertexIndices.Length == 2,
              "SSPEdge must have exactly 2 vertices.");
        }

        #endregion

    }

    public class SSPFaceTri : SSPFace
    {

        #region Properties

        public ushort VertexA
        {
            get => this[0];
            set => this[0] = value;
        }

        public ushort VertexB
        {
            get => this[1];
            set => this[1] = value;
        }

        public ushort VertexC
        {
            get => this[2];
            set => this[2] = value;
        }

        #endregion

        #region Constructor

        public SSPFaceTri(ushort[] vertexIndices)
          : base(vertexIndices)
        {
            Assert(vertexIndices.Length == 3,
              "SSPFaceTri must have exactly 3 vertices.");
        }

        #endregion

    }

    public class SSPFaceQuad : SSPFace
    {

        #region Properties

        public ushort VertexA
        {
            get => this[0];
            set => this[0] = value;
        }

        public ushort VertexB
        {
            get => this[1];
            set => this[1] = value;
        }

        public ushort VertexC
        {
            get => this[2];
            set => this[2] = value;
        }

        public ushort VertexD
        {
            get => this[3];
            set => this[3] = value;
        }

        #endregion

        #region Constructor

        public SSPFaceQuad(ushort[] vertexIndices)
          : base(vertexIndices)
        {
            Assert(vertexIndices.Length == 4,
              "SSPFaceQuad must have exactly 4 vertices.");
        }

        #endregion

    }

    public class SSPFaceNgon : SSPFace
    {

        #region Constructor

        public SSPFaceNgon(ushort[] vertexIndices)
          : base(vertexIndices)
        {
        }

        #endregion

    }

}
