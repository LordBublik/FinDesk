using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace FinDesk.Converters
{
    public static class StringConverters
    {
        public static readonly IValueConverter IsNotNullOrEmpty = new IsNotNullOrEmptyConverter();
    }

    public class IsNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}