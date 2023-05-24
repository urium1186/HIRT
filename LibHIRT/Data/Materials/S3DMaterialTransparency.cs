using LibHIRT.Data.Scripting;

namespace LibHIRT.Data.Materials
{

    public class S3DMaterialTransparency
    {

        [ScriptingProperty("colorSetIdx")]
        public int ColorSetIndex { get; set; }

        [ScriptingProperty("enabled")]
        public int Enabled { get; set; }

        [ScriptingProperty("multiplier")]
        public float Multiplier { get; set; }

        [ScriptingProperty("sources")]
        public int Sources { get; set; }

    }

}
