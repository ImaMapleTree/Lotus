using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.Extensions;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public class RoleChangeEvent : IRoleChangeEvent
{
    private PlayerControl player;
    private Optional<CustomRole> originalRole;
    private CustomRole newRole;

    private Timestamp timestamp = new();

    public RoleChangeEvent(PlayerControl player, CustomRole newRole, CustomRole? originalRole = null)
    {
        this.player = player;
        this.originalRole = Optional<CustomRole>.Of(originalRole ?? player.GetCustomRole());
        this.newRole = newRole;
    }

    public PlayerControl Player() => player;

    public Optional<CustomRole> RelatedRole() => originalRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => true;

    public string Message()
    {
        return $"{originalRole.OrElse(CustomRoleManager.Default).RoleColor.Colorize(Game.GetName(player))} transformed into {newRole.RoleColor.Colorize(newRole.RoleName)}";
    }

    public CustomRole OriginalRole() => originalRole.Get();

    public CustomRole NewRole() => newRole;
}