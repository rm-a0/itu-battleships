using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Diagnostics;
using System.Globalization;

namespace BattleshipsAvalonia.Converters;

public class TileToBrushConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 2 && values[0] is int index && values[1] is string[][] tiles)
        {
            int gridSize = tiles.Length;
            int row = index / gridSize;
            int col = index % gridSize;

            if (row < tiles.Length && col < tiles[row].Length)
            {
                string tile = tiles[row][col];

                if (tile == "empty")
                {
                    return GetBrush("Secondary", "#1A3C5C");
                }
                return GetBrush("Primary", "#4DA8FF");
            }
            return GetBrush("SuccessRed", "#FF4C4C");
        }
        return GetBrush("SuccessRed", "#FF4C4C");
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
