using LibHIRT.Data.Scripting;

namespace LibHIRT.Data.Materials
{

  public class S3DMaterialUpVector
  {

    [ScriptingProperty( "angle" )]
    public float Angle { get; set; }

    [ScriptingProperty( "enabled" )]
    public bool Enabled { get; set; }

    [ScriptingProperty( "falloff" )]
    public float Falloff { get; set; }

  }

}
