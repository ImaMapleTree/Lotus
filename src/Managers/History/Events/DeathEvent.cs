using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Roles2;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public class DeathEvent : IDeathEvent
{
    private PlayerControl deadPlayer;
    private UnifiedRoleDefinition playerRole;
    private Optional<FrozenPlayer> killer;
    private Optional<UnifiedRoleDefinition> killerRole;

    private Timestamp timestamp = new();

    public DeathEvent(PlayerControl deadPlayer, PlayerControl? killer)
    {
        this.deadPlayer = deadPlayer;
        playerRole = this.deadPlayer.PrimaryRole();
        this.killer = Optional<FrozenPlayer>.Of(Game.MatchData.GetFrozenPlayer(killer));
        this.killerRole = this.killer.Map(p => Optional<UnifiedRoleDefinition>.Of(p.MyPlayer.PrimaryRole()).OrElse(p.PrimaryRoleDefinition));
    }

    public PlayerControl Player() => deadPlayer;

    public Optional<UnifiedRoleDefinition> RelatedRole() => Optional<UnifiedRoleDefinition>.NonNull(playerRole);

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => true;

    public virtual string Message()
    {
        string baseMessage = $"{Game.GetName(deadPlayer)} was {SimpleName().ToLower()}";
        return killer.Transform(klr => baseMessage + $" by {klr.Name}.", () => baseMessage + ".");
    }

    public Optional<FrozenPlayer> Instigator() => killer;

    public Optional<UnifiedRoleDefinition> InstigatorRole() => killerRole;

    public virtual string SimpleName() => ModConstants.DeathNames.Killed;
}