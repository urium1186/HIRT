using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSpartan.Grunt.Models.HaloInfinite
{
    [IsAutomaticallySerializable]
    public class ArmorTheme
    {
        public bool IsItemOwnershipRequired { get; set; }
        public bool IsKit { get; set; }
        public int TagId { get; set; }
        public IdentifierName? VariantId { get; set; }
        public CommonItemData? CommonData { get; set; }

        public ItemOptions? Coatings { get; set; }
        public ItemHelmetOptions? Helmets { get; set; }
        public ItemOptions? Visors { get; set; }
        public ItemOptions? Gloves { get; set; }
        public ItemOptions? KneePads { get; set; }
        public ItemOptions? ChestAttachments { get; set; }
        public ItemOptions? WristAttachments { get; set; }
        public ItemOptions? HipAttachments { get; set; }
        public ItemOptions? ArmorFx { get; set; }
        public ItemOptions? MythicFx { get; set; }
        public ItemOffRegionOptions? LeftShoulderPads { get; set; }
        public ItemOffRegionOptions? RightShoulderPads { get; set; }
        public CoreRegionData? CoreRegionData { get; set; }
        public SkipNode? Emblems { get; set; }

    }
}
