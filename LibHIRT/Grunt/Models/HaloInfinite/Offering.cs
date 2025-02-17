using OpenSpartan.Grunt.Models;
using OpenSpartan.Grunt.Models.HaloInfinite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Grunt.Models.HaloInfinite
{
    [IsAutomaticallySerializable]
    public class Offering
    {
        public string OfferingId { get; set; }
        public string OfferingDisplayPath { get; set; }
        public SkipNode OfferingExpirationDate { get; set; }
        public List<OfferingItem> IncludedItems { get; set; }
        public List<SkipNode> Prices { get; set; }
        public List<SkipNode> IncludedCurrencies { get; set; }
        public List<SkipNode> IncludedRewardTracks { get; set; }
        public SkipNode BoostPath { get; set; }
        public int OperationXp { get; set; }
        public int EventXp { get; set; }
        public SkipNode MatchBoosts { get; set; }
        public List<SkipNode> RewardTrackAdjustments { get; set; }

    }
}
