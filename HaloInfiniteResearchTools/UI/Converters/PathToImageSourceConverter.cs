using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace HaloInfiniteResearchTools.UI.Converters
{
    public class PathToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (path == null)
                return null;

            // Asume que el path es relativo a la carpeta de la aplicación
            string absolutePath = Path.Combine(Environment.CurrentDirectory, path);
            return new BitmapImage(new Uri(absolutePath));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
