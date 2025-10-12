using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BattleshipsAvalonia.Models;

public class CurrentScreen
{
    [JsonPropertyName("current_screen")]
    public string Screen { get; init; }
}
