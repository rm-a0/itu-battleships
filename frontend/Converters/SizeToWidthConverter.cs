using Avalonia.Data.Converters;
using System.Globalization;

namespace BattleshipsAvalonia.Converters;

public class SizeToWidthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int size)
        {
            double multiplier = 40.0;
            if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, culture, out double parsedMultiplier))
            {
                multiplier = parsedMultiplier;
            }
            return size * multiplier;
        }
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
