using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BattleshipsAvalonia.Converters;

public class PlayerTileToBrushConverter : BaseTileConverter
{
    public override object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 3 && values[0] is int index && values[1] is string[][] tiles && values[2] is int gridSize)
        {
            int row = index / gridSize;
            int col = index % gridSize;

            if (row < tiles.Length && col < tiles[row].Length)
            {
                string tile = tiles[row][col] ?? "empty";

                if (tile == "empty")
                {
                    return GetBrush("Secondary", "#1A3C5C");
                }
                if (tile.Contains("ship"))
                {
                    return GetBrush("Ternary", "#172A3C");

                }
                if (tile == "hit")
                {
                    return GetBrush("SuccessRed", "#FF4C4C");
                }
                if (tile == "miss")
                {
                    return GetBrush("InfoBlue", "#4DA8FF");
                }
            }
        }
        return GetBrush("SuccessRed", "#FF4C4C");
    }
}
