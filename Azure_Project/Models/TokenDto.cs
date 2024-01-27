using System.Text.Json.Serialization;

namespace Azure_Project.Models;

public class TokenDto
{
    [JsonPropertyName("token")]
    public string Token { get; set; }
}

