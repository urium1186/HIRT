using HaloInfiniteResearchTools.Common;
using HaloInfiniteResearchTools.ViewModels;
using HaloInfiniteResearchTools.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace HaloInfiniteResearchTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Data Members

        private IServiceProvider _serviceProvider;
        private MainView mainView;

        #endregion

        #region Properties

        public ICommand CloseWindowCommand { get; }
        public ICommand MaximizeWindowCommand { get; }
        public ICommand MinimizeWindowCommand { get; }

        #endregion

        #region Constructor

        public MainWindow(IServiceProvider serviceProvider, MainViewModel viewModel)
        {
            _serviceProvider = serviceProvider;

            CloseWindowCommand = new Command(Close);
            MaximizeWindowCommand = new Command(MaximizeWindow);
            MinimizeWindowCommand = new Command(MinimizeWindow);

            InitializeComponent();
            DataContext = viewModel;
            viewModel.Initialize();

            mainView = (MainView)_serviceProvider.GetService(typeof(MainView));
            AddChild(mainView);

        }

        #endregion

        #region Private Methods

        private void MinimizeWindow()
          => WindowState = WindowState.Minimized;

        private void MaximizeWindow()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        #endregion
    }
}
