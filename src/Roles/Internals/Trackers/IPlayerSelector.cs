using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Internals.Trackers;

public interface IPlayerSelector
{
    public VoteResult CastVote(Optional<PlayerControl> player);

    public void Reset();
}