using HaloInfiniteResearchTools.Controls;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HaloInfiniteResearchTools.UI.Modals
{
    /// <summary>
    /// Interaction logic for GetImageModal.xaml
    /// </summary>
    public partial class GetImageModal : WindowModal
    {
        /*
        public static readonly DependencyProperty UseImagenPathProperty = DependencyProperty.Register(
          nameof(UseImagenPath),
          typeof(bool
            ),
          typeof(GetImageModal),
          new PropertyMetadata(true));

        public bool UseImagenPath
        {
            get => (bool)GetValue(UseImagenPathProperty);
            set => SetValue(UseImagenPathProperty, value);
        }
        
        public static readonly DependencyProperty NotUseImagenPathProperty = DependencyProperty.Register(
          nameof(NotUseImagenPath),
          typeof(bool
            ),
          typeof(GetImageModal),
          new PropertyMetadata(false));

        public bool NotUseImagenPath
        {
            get => (bool)GetValue(NotUseImagenPathProperty);
            set => SetValue(NotUseImagenPathProperty, value);
        }

        public static readonly DependencyProperty ImagenPathProperty = DependencyProperty.Register(
          nameof(ImagenPath),
          typeof(string),
          typeof(GetImageModal),
          new PropertyMetadata("pack://application:,,,/HaloInfiniteResearchTools;component/Resources/images/item_loading.jpeg"));

        public string ImagenPath
        {
            get => (string)GetValue(ImagenPathProperty);
            set => SetValue(ImagenPathProperty, value);
        }
        */

        public static readonly DependencyProperty ScreenCaptureImageSourceProperty = DependencyProperty.Register(
          nameof(ScreenCaptureImageSource),
          typeof(BitmapSource),
          typeof(GetImageModal),
          new PropertyMetadata());

        public BitmapSource ScreenCaptureImageSource
        {
            get => (BitmapSource)GetValue(ScreenCaptureImageSourceProperty);
            set => SetValue(ScreenCaptureImageSourceProperty, value);
        }

        public GetImageModal()
        {
            ShowFooterButtons = false;
            DataContext = this;
            Title = "Armor Picture";
            InitializeComponent();
            captureCoponent.OnCaptured += CaptureCoponent_OnCaptured;
            ScreenCaptureImageSource = new BitmapImage(new Uri("pack://application:,,,/HaloInfiniteResearchTools;component/Resources/images/item_loading.jpeg"));

        }

        public BitmapSource ConvertBitmapToSource(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;
            var hBitmap = bitmap.GetHbitmap();

            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap); // Recuerda liberar el recurso
            }
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);

        private void CaptureCoponent_OnCaptured(object? sender, System.EventArgs e)
        {
            var temp = (sender as ScreenCaptureControl);
            if (temp != null)
            {
                //ImagenPath = temp.ImagenPath;
                ScreenCaptureImageSource = ConvertBitmapToSource(
                temp.Bitmap);
                /*
                NotUseImagenPath= true;
                UseImagenPath= false;
                */
            }
        }

        public static byte[] ImageSourceToBytes(ImageSource imageSource)
        {
            var encoder = new PngBitmapEncoder();
            var bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            }

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        public static ImageSource BytesToImageSource(byte[] imageData)
        {
            using (var stream = new MemoryStream(imageData))
            {
                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames[0];
            }
        }


        private void save_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Hide();
            CloseModal("asd");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Archivos de imagen (*.png;*.jpg)|*.png;*.jpg";
            if (dialog.ShowDialog() == true)
            {
                ScreenCaptureImageSource = new BitmapImage(new Uri(dialog.FileName));
            }
        }
    }
}
