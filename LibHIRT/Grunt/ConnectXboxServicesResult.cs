using OpenSpartan.Grunt.Models;

namespace LibHIRT.Grunt
{
    public class ConnectXboxServicesResult
    {
        XboxTicket extendedTicket;
        HaloInfiniteClientFix client;

        public XboxTicket ExtendedTicket { get => extendedTicket; set => extendedTicket = value; }
        public HaloInfiniteClientFix Client { get => client; set => client = value; }
    }
}
