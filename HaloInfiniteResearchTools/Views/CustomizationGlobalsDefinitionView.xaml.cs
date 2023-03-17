using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
    /// Interaction logic for CustomizationGlobalsDefinitionView.xaml
    /// </summary>
    public partial class CustomizationGlobalsDefinitionView :   View<CustomizationGlobalsDefinitionViewModel>
    {
        public CustomizationGlobalsDefinitionView()
        {
            InitializeComponent();
        }
        void initRenderModelView()
        {
            try
            {
                var viewService = ServiceProvider.GetService<IViewService>();
                CustomizationGlobalsDefinitionViewModel temp = (this.DataContext as CustomizationGlobalsDefinitionViewModel);
                var render_model = temp.File.GetRenderModel();
                if (render_model != null)
                {
                    RenderModelViewModel renderModel = new RenderModelViewModel(ServiceProvider, render_model);
                    renderModel.ThemeConfigurations = temp.File.GetThemeConfigurations();
                    renderModel.Initialize();
                    var view = (System.Windows.UIElement)viewService.GetView(renderModel);
                    gridMain.Children.Add(view);

                    Grid.SetRow(view, 1);
                    Grid.SetColumn(view, 1); ;
                }


            }
            catch (Exception ex)
            {

                throw;
            }


        }
        private void gridMain_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var viewModel = DataContext as CustomizationGlobalsDefinitionViewModel;
            if (viewModel is null)
                return;
            initRenderModelView();
        }
    }
}
