namespace Slovo_Filter_DAL.Models;

using System.Text.Json.Serialization;

public class AiScore
{
    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("meaning")]
    public string Meaning { get; set; }
}
