using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BattleshipsAvalonia.Models;

public class PlacedShip : Ship
{
    [JsonPropertyName("row")]
    public int Row { get; init; }

    [JsonPropertyName("col")]
    public int Col { get; init; }
}
