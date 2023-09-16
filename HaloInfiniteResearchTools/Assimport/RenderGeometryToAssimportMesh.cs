using Assimp;
using LibHIRT.Domain;
using System.Collections.Generic;

namespace HaloInfiniteResearchTools.Assimport
{
    public static class RenderGeometryToAssimportMesh
    {
        static public List<Mesh> GetMeshsFromRenderGemotry(RenderGeometry renderGeometry, string prefix, List<int> materialsIndexList, List<int> filterMeshs = null, bool include = false)
        {
            List<Mesh> result = new List<Mesh>();
            for (int i = 0; i < renderGeometry.Meshes.Count; i++)
            {
                if (filterMeshs != null)
                {
                    if (!include)
                    {
                        if (filterMeshs.Contains(i))
                            continue;
                    }
                    else
                    {
                        if (!filterMeshs.Contains(i))
                            continue;
                    }

                }
                result.Add(SMeshBuilder.Build(renderGeometry.Meshes[i], 0, prefix + "_mesh_" + i.ToString(), materialsIndexList));
            }
            return result;
        }

        static public List<int> AddMeshsFromRenderGemotry(Scene onScene, RenderGeometry renderGeometry, string prefix, List<int> materialsIndexList, List<int> filterMeshs = null, bool include = false)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < renderGeometry.Meshes.Count; i++)
            {
                if (filterMeshs != null)
                {
                    if (!include)
                    {
                        if (filterMeshs.Contains(i))
                            continue;
                    }
                    else
                    {
                        if (!filterMeshs.Contains(i))
                            continue;
                    }

                }
                onScene.Meshes.Add(SMeshBuilder.Build(renderGeometry.Meshes[i], 0, prefix + "_mesh_" + i.ToString(), materialsIndexList));
                result.Add(onScene.Meshes.Count - 1);
            }
            return result;
        }
    }
}
