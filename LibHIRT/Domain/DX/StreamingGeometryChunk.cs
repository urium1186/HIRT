﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Domain.DX
{
    public class StreamingGeometryChunk
    {
        public Int16 BufferIndex { get; set; }
        public Int16 AllocationPriority { get; set; }
        public Int32 BufferStart { get; set; }
        public Int32 BufferEnd { get; set; }

    }
}
