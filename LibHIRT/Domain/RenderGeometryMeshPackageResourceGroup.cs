using LibHIRT.Domain.DX;

namespace LibHIRT.Domain
{
    public struct s_render_geometry_api_resource
    {
        public RasterizerVertexBuffer[] PcVertexBuffers;
        public RasterizerIndexBuffer[] PcIndexBuffers;
        public StreamingGeometryMesh[] StreamingMeshes;
        public StreamingGeometryChunk[] StreamingChunks;
        public StreamingGeometryBuffer[] StreamingBuffers;
        public Int64 m_sharedDXResources;
        public Int64 m_sharedDXResourceRawView;
        public Int64 RuntimeData;
    }
    public class RenderGeometryMeshPackageResourceGroup
    {
        public s_render_geometry_api_resource[]? MeshResource;
    }
}
