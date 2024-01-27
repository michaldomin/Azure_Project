using System.Text.Json.Serialization;

namespace Azure_Project.Models
{
    public class TextData
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
