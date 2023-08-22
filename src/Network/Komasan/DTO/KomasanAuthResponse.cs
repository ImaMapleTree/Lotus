using System.Text.Json.Serialization;

namespace Lotus.Network.Komasan.DTO;

public class KomasanAuthResponse
{
    [JsonPropertyName("authSecret")]
    public string AuthSecret { get; set; } = null!;
}