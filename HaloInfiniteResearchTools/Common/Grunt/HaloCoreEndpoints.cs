namespace HaloInfiniteResearchTools.Common.Grunt
{
    internal class HaloCoreEndpoints
    {
        //
        // Summary:
        //     Relying party for use with the Xbox Live authentication flow, associated with
        //     the Halo Waypoint service.
        internal static readonly string HaloWaypointXstsRelyingParty = "https://prod.xsts.halowaypoint.com/";

        //
        // Summary:
        //     Game CMS origin.
        internal static readonly string GameCmsOrigin = "gamecms-hacs";

        //
        // Summary:
        //     Economy origin.
        internal static readonly string EconomyOrigin = "economy";

        //
        // Summary:
        //     Authoring origin.
        internal static readonly string AuthoringOrigin = "authoring-infiniteugc";

        //
        // Summary:
        //     Discovery origin.
        internal static readonly string DiscoveryOrigin = "discovery-infiniteugc";

        //
        // Summary:
        //     Halo Infinite lobby origin.
        internal static readonly string HaloInfiniteLobbyOrigin = "lobby-hi";

        //
        // Summary:
        //     Settings origin.
        internal static readonly string SettingsOrigin = "settings";

        //
        // Summary:
        //     Skill origin.
        internal static readonly string SkillOrigin = "skill";

        //
        // Summary:
        //     Ban processor origin.
        internal static readonly string BanProcessorOrigin = "banprocessor";

        //
        // Summary:
        //     Stats origin.
        internal static readonly string StatsOrigin = "halostats";

        //
        // Summary:
        //     Text origin.
        internal static readonly string TextOrigin = "text";

        //
        // Summary:
        //     Content HACS origin.
        internal static readonly string ContentHacsOrigin = "content-hacs";

        //
        // Summary:
        //     Halo Waypoint service domain used for all Halo API calls.
        internal static readonly string ServiceDomain = "svc.halowaypoint.com:443";

        //
        // Summary:
        //     Endpoint used to produce the Spartan token.
        internal static readonly string SpartanTokenEndpoint = "https://settings.svc.halowaypoint.com/spartan-token";

        //
        // Summary:
        //     Endpoint used to obtain the Halo Infinite endpoints.
        internal static readonly string HaloInfiniteEndpointsEndpoint = "https://settings.svc.halowaypoint.com/settings/hipc/e2a0a7c6-6efe-42af-9283-c2ab73250c48";

        //
        // Summary:
        //     Endpoint used to obtain the Halo 5 endpoints.
        internal static readonly string Halo5EndpointsEndpoint = "https://settings.svc.halowaypoint.com/settings/h5pc/a1b344c4-91a3-47f7-92f4-95784cda3cd2";
    }
}
