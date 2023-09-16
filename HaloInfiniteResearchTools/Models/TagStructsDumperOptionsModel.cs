using DeepCopy;
using HaloInfiniteResearchTools.Common;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HaloInfiniteResearchTools.Models
{
    public class TagStructsDumperOptionsModel : ObservableObject, IDeepCopy<TagStructsDumperOptionsModel>, INotifyPropertyChanged
    {
        #region Data Members
        string _lastStartAddressS = "-1";
        public static TagStructsDumperOptionsModel Default
        {
            get
            {
                return new TagStructsDumperOptionsModel
                {
                    OutputPath = "",
                    LastStartAddress = -1,
                    LastStartAddressS = "-1"
                };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Properties

        [DefaultValue("")]
        public string OutputPath { get; set; }
        [DefaultValue(-1)]
        public long LastStartAddress
        {
            get => long.Parse(_lastStartAddressS);
            set => _lastStartAddressS = value.ToString();
        }
        [JsonIgnore]
        [DefaultValue("-1")]
        public string LastStartAddressS
        {
            get { return _lastStartAddressS; }
            set
            {
                if (value != _lastStartAddressS)
                {
                    _lastStartAddressS = value;
                    OnPropertyChanged("LastStartAddressS");
                }
            }
        }




        #endregion

        #region Constructor

        public TagStructsDumperOptionsModel()
        {
            LastStartAddress = -1;
        }

        [DeepCopyConstructor]
        public TagStructsDumperOptionsModel(TagStructsDumperOptionsModel source)
        {
        }

        #endregion

    }
}
