using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace BattleshipsAvalonia.Converters;

public abstract class BaseTileConverter : IMultiValueConverter
{
    public abstract object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture);

    protected IBrush GetBrush(string resourceName, string fallbackHex)
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
