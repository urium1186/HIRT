

namespace LibHIRT.Domain
{
    public class RenderGeometry
    {
        List<ObjMesh> _meshes = new List<ObjMesh>();
        public CompressionInfo CompressionInfo { get; set; }
        public List<ObjMesh> Meshes { get => _meshes; }

        // per_mesh_node_map
        public RenderGeometry() { }
    }
}
