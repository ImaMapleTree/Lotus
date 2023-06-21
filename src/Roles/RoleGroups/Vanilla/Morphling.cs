using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Morphling : Shapeshifter
{
    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddShapeshiftOptions(base.RegisterOptions(optionStream));
}