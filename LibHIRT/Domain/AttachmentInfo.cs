using System.Numerics;

namespace LibHIRT.Domain
{
    public class AttachmentInfo
    {
        public int NodeIndex { get; set; }
        public Vector3 Translation { get; set; }
        public Vector3 rotation { get; set; }
        public Vector3 scale { get; set; }
        public Vector3 direction { get; set; }
        public AttachmentInfo() { }
    }
}
