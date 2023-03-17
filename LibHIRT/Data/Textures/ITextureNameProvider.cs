﻿using System.Collections.Generic;

namespace LibHIRT.Data.Textures
{

  public interface ITextureNameProvider
  {

    IEnumerable<string> GetTextureNames();

  }

}
