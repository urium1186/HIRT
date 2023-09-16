﻿using System;

namespace HaloInfiniteResearchTools.Common
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AcceptsFileTypeAttribute : Attribute
    {

        #region Properties

        public Type FileType { get; }

        #endregion

        #region Constructor

        public AcceptsFileTypeAttribute(Type fileType)
        {
            FileType = fileType;
        }

        #endregion

    }

}
