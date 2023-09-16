
using JSONTreeView;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HaloInfiniteResearchTools.Controls
{
    /// <summary>
    /// Interaction logic for JsonTreeView.xaml
    /// </summary>
    public partial class JsonTreeView : UserControl
    {

        public static readonly DependencyProperty JsonStringProperty =
        DependencyProperty.Register("JsonString", typeof(string), typeof(JsonTreeView), new
           PropertyMetadata("", new PropertyChangedCallback(OnJsonStringChanged)));

        public string JsonString
        {
            get { return (string)GetValue(JsonStringProperty); }
            set { SetValue(JsonStringProperty, value); }
        }

        private static void OnJsonStringChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            JsonTreeView UserControl1Control = d as JsonTreeView;
            UserControl1Control.OnJsonStringChanged(e);
        }

        private void OnJsonStringChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                RefreshJsonTree(e.NewValue.ToString());
        }

        public JsonTreeView()
        {
            InitializeComponent();
        }


        protected void RefreshJsonTree(string jsonstring)
        {
            try
            {
                MyTreeView.Items.Clear();
                MyTreeView.ProcessJson(jsonstring);
                MyTreeView.UpdateLayout();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

    }
}
