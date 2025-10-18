using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BattleshipsAvalonia.Converters;

public class TileToDisplayConverter : IValueConverter
{
    private static readonly Dictionary<string, int> ShipNameToSize = new()
    {
        { "carrier", 5 },
        { "battleship", 4 },
        { "cruiser", 3 }
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index && parameter is string[][] tiles)
        {
            int gridSize = tiles.Length;
            int row = index / gridSize;
            int col = index % gridSize;
            string tile = tiles[row][col];
            if (tile != "empty" && ShipNameToSize.TryGetValue(tile, out int size))
            {
                return size.ToString();
            }
            return string.Empty;
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
