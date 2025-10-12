using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BattleshipsAvalonia.Models;

public class PlanningData
{
    [JsonPropertyName("player_grid")]
    public Grid PlayerGrid { get; init; }

    [JsonPropertyName("all_ships")]
    public List<Ship>? AllShips { get; init; }

    [JsonPropertyName("available_ships")]
    public List<Ship>? AvailableShips { get; init; }

    [JsonPropertyName("placed_ships")]
    public List<PlacedShip>? PlacedShips { get; init; }

    [JsonPropertyName("active_ship")]
    public PlacedShip? ActiveShip { get; init; }
}
