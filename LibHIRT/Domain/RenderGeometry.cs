

namespace LibHIRT.Domain
{
    public class RenderGeometry
    {
        List<s_mesh> _meshes = new List<s_mesh>();
        public CompressionInfo CompressionInfo { get; set; }
        public List<s_mesh> Meshes { get => _meshes;  }

        // per_mesh_node_map
        public RenderGeometry() { }
    }
}
