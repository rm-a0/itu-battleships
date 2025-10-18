using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace BattleshipsAvalonia.Models;

public class Grid
{
    [JsonPropertyName("gridSize")]
    public int GridSize { get; init; }

    [JsonPropertyName("tiles")]
    public string[][] Tiles { get; init; }

    public void PrintTiles()
    {
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                string value = row < Tiles.Length && col < Tiles[row].Length ? Tiles[row][col] : "out-of-bounds";
                Console.WriteLine($"tile[{row}][{col}] = {value}");
            }
        }
    }
}
