using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader.Common
{
    public class TRSettings
    {
        static TRSettings? _instance;
        public static TRSettings Instance { get {
                if (_instance ==  null)
                    _instance = new TRSettings();   
                return _instance;
            } }

        public string ProcAsyncBaseAddr { get; set; }
        public bool AutoHook { get; internal set; }
        public bool AutoLoad { get; internal set; }
        public bool AutoPoke { get; internal set; }
        public bool FilterOnlyMapped { get; internal set; }
        public bool Opacity { get; internal set; }
        public bool AlwaysOnTop { get; internal set; }
        public bool Updater { get; internal set; }

        private TRSettings()
        {
            // to remove
            ProcAsyncBaseAddr = "HaloInfinite.exe+0x4362118";
        }
    }
}
