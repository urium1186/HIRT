

namespace LibHIRT.Domain.DX
{
    public class RasterizerVertexBuffer
    {
        public PcVertexBuffersUsage usage;
        public PcVertexBuffersFormat format;
        public sbyte stride;
        public sbyte ownsD3DResource;
        public int count;
        public int offset;
        public D3DBufferData d3dbuffer;
        public SharpDX.Direct3D11.Buffer? d3dbufferDX;
        public Int64 m_resource;
        public Int64 m_resourceView;


        public RasterizerVertexBuffer() { }
    }
}
