using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.API;
using Lotus.API.Player;
using Lotus.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public class DeathEvent : IDeathEvent
{
    private PlayerControl deadPlayer;
    private CustomRole playerRole;
    private Optional<FrozenPlayer> killer;
    private Optional<CustomRole> killerRole;

    private Timestamp timestamp = new();

    public DeathEvent(PlayerControl deadPlayer, PlayerControl? killer)
    {
        this.deadPlayer = deadPlayer;
        playerRole = this.deadPlayer.GetCustomRole();
        this.killer = Optional<FrozenPlayer>.Of(Game.MatchData.FrozenPlayer(killer));
        this.killerRole = this.killer.Map(p => Optional<CustomRole>.Of(p.MyPlayer.GetCustomRoleSafe()).OrElse(p.Role));
    }

    public PlayerControl Player() => deadPlayer;

    public Optional<CustomRole> RelatedRole() => Optional<CustomRole>.NonNull(playerRole);

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => true;

    public virtual string Message()
    {
        string baseMessage = $"{Game.GetName(deadPlayer)} was {SimpleName().ToLower()}";
        return killer.Transform(klr => baseMessage + $" by {klr.Name}.", () => baseMessage + ".");
    }

    public Optional<FrozenPlayer> Instigator() => killer;

    public Optional<CustomRole> InstigatorRole() => killerRole;

    public virtual string SimpleName() => ModConstants.DeathNames.Killed;
}