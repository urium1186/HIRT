using HaloInfiniteResearchTools.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HaloInfiniteResearchTools.Views
{
    /// <summary>
    /// Interaction logic for LevelView.xaml
    /// </summary>
    public partial class LevelView : View<LevelViewModel>
    {
        public LevelView()
        {
            InitializeComponent();
        }

        protected override void OnDisposing()
        {
            ModelViewer?.Dispose();
            base.OnDisposing();
        }
    }


}
