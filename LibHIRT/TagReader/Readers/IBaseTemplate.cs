﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader.Readers
{
    public interface IBaseTemplate
    {
        public string GetTagGroup();
        public void Load(bool force = false);
        public void ReadParameterByName();

        public bool IsLoaded();

        public string ToJson();

        public void onInstanceLoad(object instance);

        public void AddSubForOnInstanceLoad(object objMethod);

        public void RemoveSubForOnInstanceLoad(Object objMethod);
    }
}
