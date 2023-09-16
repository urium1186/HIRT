namespace HaloInfiniteResearchTools.UI.Modals
{
    /// <summary>
    /// Interaction logic for MessageModal.xaml
    /// </summary>
    public partial class MessageModal : WindowModal
    {

        public string Message { get; set; }

        public MessageModal()
        {
            InitializeComponent();
            DataContext = this;
        }

    }

}
