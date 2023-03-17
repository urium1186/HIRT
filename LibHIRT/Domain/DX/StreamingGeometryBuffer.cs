using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Domain.DX
{
   
    public class StreamingGeometryBuffer
    {
        public int BufferSize;
        public int BindFlags;
        public byte[]? TempBufferForPipeline;
    }
}
