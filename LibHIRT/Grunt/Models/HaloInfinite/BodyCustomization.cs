﻿namespace OpenSpartan.Grunt.Models.HaloInfinite
{
    [IsAutomaticallySerializable]
    public class BodyCustomization
    {
        public APIFormattedDate? LastModifiedDateUtc { get; set; }
        public string? LeftArm { get; set; }
        public string? RightArm { get; set; }
        public string? LeftLeg { get; set; }
        public string? RightLeg { get; set; }
        public string? BodyType { get; set; }
        public int? Voice { get; set; }

    }
}
