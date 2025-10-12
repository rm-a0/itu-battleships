using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BattleshipsAvalonia.Models;

public class Grid
{
    [JsonPropertyName("gridSize")]
    public int GridSize { get; init; }

    [JsonPropertyName("tiles")]
    public string[][] Tiles { get; init; }
}
