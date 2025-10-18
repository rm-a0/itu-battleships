using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BattleshipsAvalonia.Models;

public class Ship
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("size")]
    public int Size { get; init; }

    [JsonPropertyName("color")]
    public string Color { get; init; }

    [JsonPropertyName("rotation")]
    public int Rotation { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }
}
