using LibHIRT.Data.Scripting;

namespace LibHIRT.Data.Materials
{

    public class S3DMaterialHeightMap
    {

        [ScriptingProperty("colorSetIdx")]
        public int ColorSetIndex { get; set; }

        [ScriptingProperty("invert")]
        public bool Invert { get; set; }

    }

}
