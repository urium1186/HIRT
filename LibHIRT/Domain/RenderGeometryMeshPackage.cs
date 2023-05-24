namespace LibHIRT.Domain
{
    public class RenderGeometryMeshPackage
    {
        public struct ResourceLookup
        {
            public Int16 ResourceGroupIndex;
            public Int16 GroupItemIndex;
        }

        public Int16 Flags;
        public MeshResourcePackingPolicy MeshResourcePackingPolicy;
        public Int16 TotalIndexBufferCount;
        public Int16 TotalVertexBufferCount;
        public RenderGeometryMeshPackageResourceGroup[]? MeshResourceGroups;
        public ResourceLookup[]? IndexResourceLookUp;
        public ResourceLookup[]? VertexResourceLookUp;




        public RenderGeometryMeshPackage() { }
    }
}
