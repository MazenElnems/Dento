using System.Text.Json.Serialization;

namespace Dento.DTOs;
public sealed class PaymobSourceDataDto
{
    [JsonPropertyName("pan")]
    public string Pan { get; set; } = string.Empty;

    [JsonPropertyName("sub_type")]
    public string SubType { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}