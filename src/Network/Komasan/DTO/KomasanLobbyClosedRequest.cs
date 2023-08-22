namespace Lotus.Network.Komasan.DTO;

public class KomasanLobbyClosedRequest : KomasanLobbyResponse
{
    public string Reason { get; set; }

    public KomasanLobbyClosedRequest(string reason)
    {
        Reason = reason;
    }
}