

namespace LibHIRT.Domain
{
    public class Flags
    {
        int _intValue;
        Dictionary<string, bool> options;
        public Flags(int value)
        {
            _intValue = value;
        }

        public Dictionary<string, bool> Options
        {
            get
            {
                if (options == null)
                    options = new Dictionary<string, bool>();
                return options;
            }
        }

        public int Value { get => _intValue; }
    }
}
