
namespace LibHIRT.Domain
{
    public class s_mesh
    {
        List<LODRenderData> _LODRenderData;
        public string Name { get; set; }

        public int RigidNodeIndex { get; set; }
        public int CloneIndex { get; set; }
        public bool UseDualQuat { get; set; }

        public Flags MeshFlags { get; set; }
        public VertType VertType { get; set; }
        public IndexBufferType IndexBufferType { get; set; }
        public AttachmentInfo AttachmentInfo { get; set; }


        public List<LODRenderData> LODRenderData
        {
            get
            {
                if (_LODRenderData==null)
                    _LODRenderData= new List<LODRenderData>();    
                return _LODRenderData;
            }
        }
    }
}
