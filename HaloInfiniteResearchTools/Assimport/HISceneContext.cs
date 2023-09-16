using Assimp;
using LibHIRT.Domain;
using System.Collections.Generic;

namespace HaloInfiniteResearchTools.Assimport
{
    public class HISceneContext : ISceneContext
    {
        private readonly Scene scene;
        private Dictionary<string, MapMeshInContext> mapMeshIn;


        public HISceneContext(string name)
        {
            scene = new Scene();
            Scene.Materials.Add(new Material() { Name = "DefaultMaterial" });
            mapMeshIn = new Dictionary<string, MapMeshInContext>();
        }

        public Scene Scene => scene;

        public Node AddRenderGeometry(string nameToMap, RenderGeometry renderGeometry)
        {
            return AddRenderGeometry(nameToMap, renderGeometry, null, null);
        }

        public Node AddRenderGeometry(string nameToMap, RenderGeometry renderGeometry, List<int> materials, List<int> filter, bool include = false)
        {
            MapMeshInContext temp = new MapMeshInContext(nameToMap);
            temp.Meshs.AddRange(RenderGeometryToAssimportMesh.AddMeshsFromRenderGemotry(scene, renderGeometry, nameToMap, materials, filter, include));
            Node tempNode = new Node(nameToMap);
            tempNode.MeshIndices.AddRange(temp.Meshs);
            mapMeshIn[nameToMap] = temp;
            return tempNode;
        }
    }
}
