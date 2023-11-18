namespace OpenSpartan.Grunt.Models.HaloInfinite
{
    [IsAutomaticallySerializable]
    public class ArmorCoreAttach
    {
        public int TagId { get; set; }
        public MarkerLocation? MarkerLocation { get; set; }
        public IdentifierName? StyleId { get; set; }
        public int MeshIndex { get; set; }
        public CommonItemData? CommonData { get; set; }
    }

    [IsAutomaticallySerializable]
    public class MarkerLocation
    {

        public IdentifierName? MarkerName { get; set; }
        public IdentifierName? VariantId { get; set; }

    }


}
