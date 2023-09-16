using System;

namespace HaloInfiniteResearchTools.UI.Modals
{
    /// <summary>
    /// Interaction logic for ExceptionModal.xaml
    /// </summary>
    public partial class ExceptionModal : WindowModal
    {

        public ExceptionModal(Exception exception)
        {
            InitializeComponent();
            DataContext = exception;
        }

    }
}
