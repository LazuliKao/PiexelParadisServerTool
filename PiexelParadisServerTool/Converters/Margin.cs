using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Elements.Converters
{
    public class MarginToLeft : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new Thickness((double)value, 0, 0, 0);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    public class MarginToTop : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new Thickness(0, (double)value, 0, 0);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    public class MarginToRight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new Thickness(0, 0, (double)value, 0);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    public class MarginToBottom : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new Thickness(0, 0, 0, (double)value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    public class MaeginToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((Thickness)value).Top;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
