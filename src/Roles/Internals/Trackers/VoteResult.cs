using Lotus.Chat;

namespace Lotus.Roles.Internals.Trackers;

public class VoteResult
{
    public VoteResultType VoteResultType { get; }
    public byte Selected { get; }
    private ChatHandler chatHandler;


    public VoteResult(VoteResultType resultType, byte selected ,string? message = null)
    {
        chatHandler = ChatHandler.Of(message).LeftAlign();
        Selected = selected;
        VoteResultType = resultType;
    }

    public ChatHandler Message() => chatHandler;

    public void Send(PlayerControl? player = null, string? title = null)
    {
        if (title != null) chatHandler = chatHandler.Title(title);
        chatHandler.Send(player);
    }
}

public enum VoteResultType
{
    None,
    Skipped,
    Selected,
    Unselected,
    Confirmed
}