using OpenSpartan.Grunt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
