using SharpDX.Direct3D11;
using SharpDX.Direct3D9;


namespace LibHIRT.Domain.DX
{
    public struct D3DBufferData
    {
        public int ByteWidth;
        public BindFlags BindFlags;
        //public Usage Usage;
        public Int16 Usage;
        public ResourceMiscFlags MiscFlags;
        //public CpuAccessFlags CPUAccessFlags;
        public Int16 CPUAccessFlags;
        public byte[] D3dBuffer;
        public int StructureByteStride;
    }
}
