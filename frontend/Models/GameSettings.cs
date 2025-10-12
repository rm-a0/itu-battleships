using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BattleshipsAvalonia.Models;

public class GameSettings
{
    [JsonPropertyName("selectedBoard")]
    public string SelectedBoard { get; init; }
}
