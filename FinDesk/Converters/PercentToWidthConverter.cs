using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace FinDesk.Converters
{
    public class PercentToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                double multiplier = 3.0; 
                if (parameter is string paramStr && double.TryParse(paramStr, out double parsed))
                {
                    multiplier = parsed;
                }
                return percent * multiplier;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AmountToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                double multiplier = 0.01;
                if (parameter is string paramStr && double.TryParse(paramStr, out double parsed))
                {
                    multiplier = parsed;
                }
                return (double)(amount * (decimal)multiplier);
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}