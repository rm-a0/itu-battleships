using Avalonia.Data.Converters;
using System.Globalization;
using BattleshipsAvalonia.ViewModels;

namespace BattleshipsAvalonia.Converters;

public class IndexToCellPositionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index && parameter is int gridSize)
        {
            return new CellPosition(index / gridSize, index % gridSize);
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
