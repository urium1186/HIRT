using HaloInfiniteResearchTools.Common;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Point = System.Drawing.Point;

namespace HaloInfiniteResearchTools.Controls
{
    /// <summary>
    /// Interaction logic for ScreenCaptureControl.xaml
    /// </summary>
    public partial class ScreenCaptureControl : UserControl
    {
        public event EventHandler OnCaptured;
        private FullScreenWindow fullScreenWindow;

        private Bitmap _bitmap;

        public string ImagenPath { get; private set; }
        public Bitmap Bitmap { get => _bitmap; set => _bitmap = value; }

        public ScreenCaptureControl()
        {
            InitializeComponent();

        }


        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            Win32.SwitchToApp("HaloInfinite");
            // Captura la pantalla completa
            /*var screenCapture = CaptureScreen();

            // Convierte la captura de pantalla a un ImageSource que puede ser usado en WPF
            var screenCaptureImageSource = Imaging.CreateBitmapSourceFromHBitmap(
                screenCapture.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());*/
            // Comprueba si la ventana ya existe
            if (fullScreenWindow == null || !fullScreenWindow.IsVisible)
            {
                // Crea una nueva ventana
                fullScreenWindow = new FullScreenWindow();
                fullScreenWindow.Closed += FullScreenWindow_Closed;
                // Establece la captura de pantalla como fondo de la ventana
                //fullScreenWindow.Background = new ImageBrush(screenCaptureImageSource);
                fullScreenWindow.Show();
            }
            else
            {
                // La ventana ya existe, así que tráela al frente
                fullScreenWindow.Activate();
            }

            // Hacer la ventana transparente
            fullScreenWindow.Opacity = 0.3;
        }

        private void FullScreenWindow_Closed(object? sender, EventArgs e)
        {
            CaptureScreenCorp(fullScreenWindow.CropArea);
            Win32.SwitchToApp("HaloInfiniteResearchTools");
            if (OnCaptured != null)
                OnCaptured(this, EventArgs.Empty);

        }

        public static Bitmap CaptureScreen()
        {
            var screenBounds = System.Windows.Forms.Screen.GetBounds(System.Drawing.Point.Empty);

            using (var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(Point.Empty, Point.Empty, screenBounds.Size);
                }

                return (Bitmap)bitmap.Clone();
            }
        }

        public void CaptureScreenCorp(System.Drawing.Rectangle cropArea)
        {
            if (cropArea == System.Drawing.Rectangle.Empty)
                return;
            _bitmap = new Bitmap(cropArea.Width, cropArea.Height);
            using (var graphics = Graphics.FromImage(_bitmap))
            {
                graphics.CopyFromScreen(cropArea.Left, cropArea.Top, 0, 0, cropArea.Size);
            }
            /**/
            string tempPath = System.IO.Path.GetTempPath();
            ImagenPath = System.IO.Path.Combine(tempPath, "screenshot.png");
            /*if (System.IO.File.Exists(ImagenPath))
            {
                System.IO.File.Delete(ImagenPath);
            }
            _bitmap.Save(ImagenPath, ImageFormat.Png);*/
        }
    }
}
