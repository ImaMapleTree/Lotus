using VentLib.Utilities.Optionals;

namespace Lotus.API.Reactive.HookEvents;

public class CastVoteHookEvent: IHookEvent
{
    public PlayerControl Voter;
    public Optional<PlayerControl> Vote;

    public CastVoteHookEvent(PlayerControl voter, Optional<PlayerControl> vote)
    {
        this.Voter = voter;
        this.Vote = vote;
    }
}