namespace LibHIRT.Utils
{
    public class UIDebug
    {
        public static Dictionary<string, Dictionary<object, List<object>>> debugValues = new Dictionary<string, Dictionary<object, List<object>>>();
    }

    public static class DebugConfig
    {
        public static bool NoCheckFails = true;
    }
}
