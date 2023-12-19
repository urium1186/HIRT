﻿using System.Collections.Generic;

namespace HaloInfiniteResearchTools.Assimport
{
    public class MapMeshInContext
    {
        string name;
        List<int> meshs;
        List<int> materials;

        MapMeshInContext subContext;
        public MapMeshInContext(string name)
        {
            this.name = name;
            meshs = new List<int>();
            materials = new List<int>();
        }

        public List<int> Meshs { get => meshs; }
        public List<int> Materials { get => materials; }
        public string Name { get => name; }
    }
}