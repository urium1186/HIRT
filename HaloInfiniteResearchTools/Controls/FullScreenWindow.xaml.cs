using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace HaloInfiniteResearchTools.Controls
{
    /// <summary>
    /// Interaction logic for FullScreenWindow.xaml
    /// </summary>
    public partial class FullScreenWindow : Window
    {
        private System.Windows.Point startPoint;
        private System.Windows.Shapes.Rectangle rectangle;

        private System.Drawing.Rectangle cropArea;

        public System.Drawing.Rectangle CropArea { get => cropArea; set => cropArea = value; }

        public FullScreenWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += CaptureWindow_MouseLeftButtonDown;
            this.MouseMove += CaptureWindow_MouseMove;
            this.MouseLeftButtonUp += CaptureWindow_MouseLeftButtonUp;

        }

        private void CaptureWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);

            rectangle = new System.Windows.Shapes.Rectangle
            {
                Stroke = System.Windows.Media.Brushes.Red,
                StrokeThickness = 2
            };
            Canvas.SetLeft(rectangle, startPoint.X);
            Canvas.SetTop(rectangle, startPoint.Y);
            canvas.Children.Add(rectangle);
        }

        private void CaptureWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || rectangle == null)
                return;

            var pos = e.GetPosition(canvas);

            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            rectangle.Width = w;
            rectangle.Height = h;

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
        }

        private void CaptureWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (rectangle != null)
            {
                // Aquí puedes usar la biblioteca ScreenCapture para capturar la región de la pantalla
                // que corresponde al rectángulo que el usuario dibujó.
                // Convertir las coordenadas del rectángulo a coordenadas de pantalla

                var point = rectangle.PointToScreen(new System.Windows.Point(0, 0));
                var x = (int)point.X;
                var y = (int)point.Y;
                var width = (int)rectangle.ActualWidth;
                var height = (int)rectangle.ActualHeight;


                this.cropArea = new System.Drawing.Rectangle(x, y, width, height);


                // Limpiar el rectángulo
                this.canvas.Children.Remove(rectangle);
                rectangle = null;

            }
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
