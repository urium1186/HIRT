using System.Collections;

namespace LibHIRT.Data
{

    public class S3DScene
    {

        public uint PropertyCount { get; set; }
        public BitArray PropertyFlags { get; set; }

        public List<string> TextureList { get; set; }
        public string PS { get; set; }
        public List<string> InstMaterialInfoList { get; set; }

        public S3DGeometryGraph GeometryGraph { get; set; }

    }

}
