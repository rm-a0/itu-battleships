using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Diagnostics;
using System.Globalization;

namespace BattleshipsAvalonia.Converters;

public class IsShipSizeSelectedConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 2 && values[0] is int groupSize && values[1] is int activeShipSize)
        {
            return groupSize == activeShipSize
                ? GetBrush("Primary", "#4DA8FF")
                : GetBrush("TextPrimary", "#E6ECEF");
        }
        return GetBrush("TextPrimary", "#E6ECEF");
    }

    private IBrush GetBrush(string resourceName, string fallbackHex)
    {
        try
        {
            return Application.Current?.FindResource(resourceName) as IBrush
                ?? new SolidColorBrush(Color.Parse(fallbackHex));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching brush '{resourceName}': {ex.Message}");
            return new SolidColorBrush(Color.Parse(fallbackHex));
        }
    }
}
