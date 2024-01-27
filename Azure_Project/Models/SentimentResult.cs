using System.Text.Json.Serialization;

namespace Azure_Project.Models;

public class SentimentResult
{
    [JsonPropertyName("polarity")]
    public double? Polarity { get; set; }

    [JsonPropertyName("subjectivity")]
    public double? Subjectivity { get; set; }
}