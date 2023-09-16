using LibHIRT.Domain.Geometry;


namespace LibHIRT.Domain
{
    public class LODRenderData
    {
        public s_part[] Parts { get; set; }
        public s_subpart[] SubParts { get; set; }

        public SSPVertex[] Vertexs { get; set; }

        public Flags LodFlags { get; set; }
        public Flags LodRenderFlags { get; set; }

        Dictionary<BufferVertType, int[]> _vertexBufferIndices;
        List<uint> _indexBufferIndex;
        public int Vert_count
        {
            get => vert_count;
            set
            {
                vert_count = value;
                vertexs = new SSPVertex[vert_count];
            }
        }

        public string Name { get; set; }

        List<SSPFace> _faces;

        public Dictionary<BufferVertType, int[]> VertexBufferIndices
        {
            get
            {
                if (_vertexBufferIndices == null)
                    _vertexBufferIndices = new Dictionary<BufferVertType, int[]>();
                return _vertexBufferIndices;
            }
        }

        public List<uint> IndexBufferIndex
        {
            get
            {
                if (_indexBufferIndex == null)
                    _indexBufferIndex = new List<uint>();
                return _indexBufferIndex;
            }
        }

        public List<SSPFace> Faces
        {
            get
            {
                if (_faces == null)
                    _faces = new List<SSPFace>();
                return _faces;
            }
        }

        int vert_count = 0;
        SSPVertex[] vertexs;
        private s_mesh _meshContainer;

        public LODRenderData(s_mesh mesh)
        {
            _meshContainer = mesh;
        }
    }
}
