using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader.Headers
{
    public enum TagStructType {
        Root = 0,
    Tagblock = 1,
    ExternalFileDescriptor = 2,
    ResourceHandle = 3,
    NoDataStartBlock = 4,
    }

    public enum TargetPlatform {
        pc = 3
    }

    public enum PcVertexBuffersFormat {
        real = 0,
    realVector2D = 1,
    realVector3D = 2,
    realVector4D = 3,
    byteVector4D = 4,
    byteARGBColor = 5,
    shortVector2D = 6,
    shortVector2DNormalized = 7,
    shortVector4DNormalized = 8,
    wordVector2DNormalized = 9,
    wordVector4DNormalized = 10,
    real16Vector2D = 11,
    real16Vector4D = 12,
    f_10_10_10_normalized = 13,
    f_10_10_10_2 = 14,
    f_10_10_10_2_signedNormalizedPackedAsUnorm = 15,
    dword = 16,
    dwordVector2D = 17,
    f_11_11_10_float = 18,
    byteUnitVector3D = 19,
    wordVector3DNormalizedWith4Word = 20,
    }

    public enum PcVertexBuffersUsage {
        Position = 0,
    UV0 = 1,
    UV1 = 2,
    UV2 = 3,
    Color = 4,
    Normal = 5,
    Tangent = 6,
    BlendIndices0 = 7,
    BlendWeights0 = 8,
    BlendIndices1 = 9,
    BlendWeights1 = 10,
    PrevPosition = 11,
    InstanceData = 12,
    BlendshapePosition = 13,
    BlendshapeNormal = 14,
    BlendshapeIndex = 15,
    Edge = 16,
    EdgeIndex = 17,
    EdgeIndexInfo = 18,
    }

    
    public class Common
    {
        
    }
}
