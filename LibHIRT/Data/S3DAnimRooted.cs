﻿using System.Numerics;

namespace LibHIRT.Data
{

  public class S3DAnimRooted
  {

    public Vector3 IniTranslation { get; set; }
    public M3DSpline PTranslation { get; set; }
    public Vector4 IniRotation { get; set; }
    public M3DSpline PRotation { get; set; }

  }

}
