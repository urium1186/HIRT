using System.Windows.Controls;
using HaloInfiniteResearchTools.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using HaloInfiniteResearchTools.Models;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using PropertyChanged;
using MaterialCore = HelixToolkit.SharpDX.Core.Model.MaterialCore;
using SharpDX;
using System.Windows;
using HaloInfiniteResearchTools.Services.Abstract;
using LibHIRT.Domain;
using LibHIRT.Processes;
using System.IO;
using HaloInfiniteResearchTools.Processes.Utils;
using HelixToolkit.SharpDX.Core.Model;
using LibHIRT.TagReader;
using System.Security.Policy;

namespace HaloInfiniteResearchTools.Controls
{
    /// <summary>
    /// Interaction logic for Model3DViewerControl.xaml
    /// </summary>
    public partial class Model3DViewerControl : UserControl, IDisposable
    {
        public static readonly DependencyProperty SceneContextProperty = DependencyProperty.Register("SceneContex", typeof(ISceneContext), typeof(Model3DViewerControl), new
        PropertyMetadata(null, new PropertyChangedCallback(OnSceneContexChanged)));

        public ISceneContext SceneContex
        {
            get { return (ISceneContext)GetValue(SceneContextProperty); }
            set { SetValue(SceneContextProperty, value); }
        }
        private static async void OnSceneContexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Model3DViewerControl userCtrl = d as Model3DViewerControl;
            userCtrl.OnSceneContexChanged(e);
        }
        private async void OnSceneContexChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is RenderGeometrySceneContext)
            {
                var temp = e.NewValue as RenderGeometrySceneContext;
                //await PrepareModelViewer(temp.Scene);
                BindingOperations.GetBindingExpressionBase((ModelViewerControl)ModelViewer, ModelViewerControl.ModelProperty).UpdateTarget();
            }
        }

        public string StringName {get; set;}

        public Model3DViewerControl()
        {
            StringName = "Meches";
            InitializeComponent();
        }

        

        #region public Meth
        public void LoadRenderGeometry(RenderGeometry renderGeometry, string prefix,bool resetView = true) {
            
        }
        #endregion

        #region Private Methods
        public void Dispose()
        {
            ModelViewer?.Dispose();
        }

        private void UpdateColumnsWidth(ListView listView)
        {
            if (listView is null)
                return;

            var gridView = listView.View as GridView;
            if (gridView is null)
                return;

            var lastColumnIdx = gridView.Columns.Count - 1;
            if (listView.ActualWidth == double.NaN)
                listView.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var remainingSpace = listView.ActualWidth;
            for (int i = 0; i < gridView.Columns.Count; i++)
                if (i != lastColumnIdx)
                    remainingSpace -= gridView.Columns[i].ActualWidth;

            gridView.Columns[lastColumnIdx].Width = remainingSpace >= 0 ? remainingSpace : 0;
        }

        #endregion

        #region Event Handlers

        private void OnContextMenuLoaded(object sender, RoutedEventArgs e)
        {
            (sender as ContextMenu).DataContext = this.DataContext;
        }

        private void OnListViewSizeChanged(object sender, SizeChangedEventArgs e)
          => UpdateColumnsWidth(sender as ListView);

        private void OnListViewLoaded(object sender, RoutedEventArgs e)
          => UpdateColumnsWidth(sender as ListView);

        #endregion

    }


}
