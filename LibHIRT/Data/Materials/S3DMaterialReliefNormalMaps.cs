using LibHIRT.Data.Scripting;

namespace LibHIRT.Data.Materials
{

    public class S3DMaterialReliefNormalMaps
    {

        [ScriptingProperty("macro")]
        public S3DMaterialNormalMap Macro { get; set; }

        [ScriptingProperty("micro1")]
        public S3DMaterialNormalMap Micro1 { get; set; }

        [ScriptingProperty("micro2")]
        public S3DMaterialNormalMap Micro2 { get; set; }

    }

}
