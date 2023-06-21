using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;

namespace Lotus.Roles.RoleGroups.Madmates.Roles;

public class MadGuardian : MadCrewmate
{
    [RoleAction(RoleActionType.Interaction)]
    private void MadGuardianAttacked(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (interaction.Intent is not (IFatalIntent or IHostileIntent)) return;
        handle.Cancel();
    }
}