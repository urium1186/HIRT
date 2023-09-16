using LibHIRT.TagReader;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HaloInfiniteResearchTools.UI.Converters
{

    public class ColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = Color.FromArgb(
         255, // Specifies the transparency of the color.
         255, // Specifies the amount of red.
         255, // specifies the amount of green.
         0); //
            if (value != null)
            {
                if (value.GetType() == typeof(ARGB))
                {
                    var r = (ARGB)value;

                    color.A = byte.Parse(Math.Ceiling(r.A_value * 254).ToString());
                    color.ScR = r.R_value;
                    color.ScG = r.G_value;
                    color.ScB = r.B_value;

                }
                else if (value.GetType() == typeof(RGB))
                {
                    var r = (RGB)value;

                    color.A = 255;
                    color.ScR = r.R_value;
                    color.ScG = r.G_value;
                    color.ScB = r.B_value;

                }
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}
