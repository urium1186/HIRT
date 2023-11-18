namespace LibHIRT.Grunt.Util
{
    //
    // Summary:
    //     Constants that do not change within the application.
    internal class GlobalConstants
    {
        //
        // Summary:
        //     User agent for the web-based requests to the Halo Waypoint API.
        internal static readonly string WEB_USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36 Edg/105.0.1343.27";

        //
        // Summary:
        //     Halo Waypoint user agent used for outbound HTTP API requests.
        internal static readonly string HALO_WAYPOINT_USER_AGENT = "HaloWaypoint/2021112313511900 CFNetwork/1327.0.4 Darwin/21.2.0";

        //
        // Summary:
        //     Halo Infinite PC game user agent used for outbound HTTP API requests.
        internal static readonly string HALO_PC_USER_AGENT = "SHIVA-2043073184/6.10021.18539.0 (release; PC)";

        //
        // Summary:
        //     Default scopes used for the Xbox Live authentication.
        internal static readonly string[] DEFAULT_AUTH_SCOPES = new string[2] { "Xboxlive.signin", "Xboxlive.offline_access" };
    }
}
