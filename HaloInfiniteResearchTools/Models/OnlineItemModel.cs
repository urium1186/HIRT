using HaloInfiniteResearchTools.Common;

namespace HaloInfiniteResearchTools.Models
{
    public class OnlineItemModel : ObservableObject
    {
        private string _imageSource;
        public string ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }

        private string _description;

        public OnlineItemModel()
        {
            _imageSource = "pack://application:,,,/HaloInfiniteResearchTools;component/Resources/images/item_loading.jpeg";
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }
    }
}
