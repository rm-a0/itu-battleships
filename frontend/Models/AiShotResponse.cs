// Author: Michal Repcik (xrepcim00)
using System.Text.Json.Serialization;

namespace BattleshipsAvalonia.Models;

public class AiShotResponse
{
    [JsonPropertyName("row")]
    public int Row { get; init; }

    [JsonPropertyName("col")]
    public int Col { get; init; }

    [JsonPropertyName("result")]
    public string Result { get; init; } = "miss";
}
