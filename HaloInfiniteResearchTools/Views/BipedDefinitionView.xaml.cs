using HaloInfiniteResearchTools.Services;
using HaloInfiniteResearchTools.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;

namespace HaloInfiniteResearchTools.Views
{
    /// <summary>
    /// Interaction logic for BipedDefinitionView.xaml
    /// </summary>
    public partial class BipedDefinitionView : View<BipedDefinitionViewModel>
    {
        public BipedDefinitionView()
        {
            InitializeComponent();
        }

        void initRenderModelView()
        {
            try
            {
                var viewService = ServiceProvider.GetService<IViewService>();
                BipedDefinitionViewModel temp = (this.DataContext as BipedDefinitionViewModel);
                var render_model = temp?.File?.GetRenderModel();
                if (render_model != null)
                {
                    RenderModelViewModel renderModel = new RenderModelViewModel(ServiceProvider, render_model);
                    renderModel.ModelInfo = temp.File.GetModelVariants();
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
            var viewModel = DataContext as BipedDefinitionViewModel;
            if (viewModel is null)
                return;
            initRenderModelView();
        }

    }
}
