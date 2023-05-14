using Lotus.API;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Mafia: Impostor
{
    public override bool CanKill() => GameStates.CountAliveImpostors() <= 1;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => CanKill() && base.TryKill(target);
}