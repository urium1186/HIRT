﻿using System;

namespace LibHIRT.Data.Scripting
{

  [AttributeUsage( AttributeTargets.Property, AllowMultiple = true )]
  public class ScriptingPropertyAttribute : Attribute
  {

    public readonly string PropertyName;
    public readonly int ArrayIndex;

    public ScriptingPropertyAttribute( string propertyName, int arrayIndex = -1 )
    {
      PropertyName = propertyName;
      ArrayIndex = arrayIndex;
    }

  }

}
