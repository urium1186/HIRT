using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Utils
{
    public class UIDebug
    {
        public static Dictionary<string, Dictionary<object, List<object>>> debugValues = new Dictionary<string, Dictionary<object, List<object>>>();
    }

    public static class DebugConfig{
        public static bool NoCheckFails = false;
    }
}
