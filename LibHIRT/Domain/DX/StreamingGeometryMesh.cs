using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
