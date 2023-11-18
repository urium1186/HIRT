using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HaloInfiniteResearchTools.UI.Converters
{

    [ValueConversion(typeof(ICollection), typeof(Visibility))]
    public class CollectionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection collection)
            { 
                if (parameter!= null)
                    return collection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                else
                    return collection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
                
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          => Convert(value, targetType, parameter, culture);
    }

    [ValueConversion(typeof(Visibility), typeof(int))]
    public class IntVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int isVisible)
                return isVisible > 0 ? Visibility.Visible : Visibility.Hidden;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          => Convert(value, targetType, parameter, culture);
    }

    [ValueConversion(typeof(Visibility), typeof(bool))]
    public class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible) {
                
                if (parameter != null && bool.TryParse(parameter.ToString(), out bool result)) {
                    if (result) {
                        isVisible = !isVisible;
                    }    
                }
                    
                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          => Convert(value, targetType, parameter, culture);
    }

    [ValueConversion(typeof(Visibility), typeof(object))]
    public class EqualVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          => Convert(value, targetType, parameter, culture);
    }

    public class NoEqualVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value.Equals(parameter) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
          => Convert(value, targetType, parameter, culture);
    }

}
