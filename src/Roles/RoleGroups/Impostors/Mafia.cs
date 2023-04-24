using TOHTOR.API;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;

namespace TOHTOR.Roles.RoleGroups.Impostors;

public class Mafia: Impostor
{
    public override bool CanKill() => GameStates.CountAliveImpostors() <= 1;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => CanKill() && base.TryKill(target);
}