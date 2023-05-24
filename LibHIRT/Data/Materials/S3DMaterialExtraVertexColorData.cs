using LibHIRT.Data.Scripting;

namespace LibHIRT.Data.Materials
{

    public class S3DMaterialExtraVertexColorData
    {

        [ScriptingProperty("colorA")]
        public S3DMaterialColor ColorA { get; set; }

        [ScriptingProperty("colorB")]
        public S3DMaterialColor ColorB { get; set; }

        [ScriptingProperty("colorG")]
        public S3DMaterialColor ColorG { get; set; }

        [ScriptingProperty("colorR")]
        public S3DMaterialColor ColorR { get; set; }

    }

}
