using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BattleshipsAvalonia.Converters
{
    public class SizeToRangeConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int size && size > 0)
            {
                return Enumerable.Range(0, size);
            }
            return Enumerable.Empty<int>();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Converting from range to size is not supported.");
        }
    }
}
