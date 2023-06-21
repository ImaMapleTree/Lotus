namespace Lotus.Managers.Models;

public class BannedPlayer
{
    public string FriendCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Reason { get; set; } = "None Specified";
    public long Identifier { get; set; }

    public BannedPlayer()
    {

    }

    public BannedPlayer(string? reason)
    {
        if (reason != null) Reason = reason;
    }
}