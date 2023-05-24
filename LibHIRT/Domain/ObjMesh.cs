
namespace LibHIRT.Domain
{
    public class ObjMesh
    {
        List<MeshLOD> _LODRenderData;
        public string Name { get; set; }

        public int RigidNodeIndex { get; set; }
        public int CloneIndex { get; set; }
        public bool UseDualQuat { get; set; }

        public Flags MeshFlags { get; set; }
        public VertType VertType { get; set; }
        public IndexBufferType IndexBufferType { get; set; }
        public AttachmentInfo AttachmentInfo { get; set; }


        public List<MeshLOD> LODRenderData
        {
            get
            {
                if (_LODRenderData == null)
                    _LODRenderData = new List<MeshLOD>();
                return _LODRenderData;
            }
        }
    }
}
