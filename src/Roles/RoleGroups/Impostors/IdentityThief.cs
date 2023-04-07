
using TOHTOR.Extensions;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Impostors;

public class IdentityThief : Impostor
{
    private bool shiftsUntilNextKill;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        bool killed = base.TryKill(target);
        if (killed) MyPlayer.RpcShapeshift(target, true);
        if (shiftsUntilNextKill) Async.Schedule(() => MyPlayer.RpcRevertShapeshift(true), KillCooldown);
        return killed;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Shifts Until Next Kill")
                .AddOnOffValues()
                .BindBool(b => shiftsUntilNextKill = b)
                .Build());
}