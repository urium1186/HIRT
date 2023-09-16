namespace LibHIRT.Domain.DX
{
    public struct StreamingChunkList
    {
        public byte[]? Chunks;
    }
    public class StreamingGeometryMesh
    {
        public int LodStateCacheSlot;
        public sbyte RequiredLod;
        public StreamingChunkList[]? MeshLodChunks;
    }
}
