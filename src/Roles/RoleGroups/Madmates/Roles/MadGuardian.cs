using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;

namespace TOHTOR.Roles.RoleGroups.Madmates.Roles;

public class MadGuardian : MadCrewmate
{
    [RoleAction(RoleActionType.Interaction)]
    private void MadGuardianAttacked(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (interaction.Intent() is not (IFatalIntent or IHostileIntent)) return;
        handle.Cancel();
    }
}