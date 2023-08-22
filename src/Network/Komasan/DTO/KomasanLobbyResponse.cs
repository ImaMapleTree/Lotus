using System.Text.Json.Serialization;

namespace Lotus.Network.Komasan.DTO;

public class KomasanLobbyResponse
{
    [JsonPropertyName("refreshIn")]
    public long RefreshIn { get; set; }
}