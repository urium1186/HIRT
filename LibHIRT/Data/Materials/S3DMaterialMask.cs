using LibHIRT.Data.Scripting;

namespace LibHIRT.Data.Materials
{

    public class S3DMaterialMask
    {

        [ScriptingProperty("textureName")]
        public string TextureName { get; set; }

        [ScriptingProperty("tilingU")]
        public float TilingU { get; set; }

        [ScriptingProperty("tilingV")]
        public float TilingV { get; set; }

        [ScriptingProperty("uvSetIdx")]
        public int UvSetIndex { get; set; }

    }

}
