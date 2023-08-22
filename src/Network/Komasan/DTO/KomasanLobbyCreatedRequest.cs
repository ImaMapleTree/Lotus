using System.Text.Json.Serialization;

namespace Lotus.Network.Komasan.DTO;

public class KomasanLobbyCreatedRequest
{
    [JsonPropertyName("hostName")]
    public string HostName { get; set; }
    [JsonPropertyName("region")]
    public AURegion Region { get; set; }

    [JsonPropertyName("modName")]
    public string ModName { get; set; }
    [JsonPropertyName("modVersion")]
    public string ModVersion { get; set; }

    [JsonPropertyName("players")]
    public int Players { get; set; }
    [JsonPropertyName("secondsRemaining")]
    public int SecondsRemaining { get; set; }
}