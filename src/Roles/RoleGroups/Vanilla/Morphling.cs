using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.RPC;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Morphling : Shapeshifter
{
    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(RoleActionType.Shapeshift, Subclassing = false)]
    public void Shapeshift(PlayerControl target) => MyPlayer.CRpcShapeshift(target, true);

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Shapeshift Cooldown")
                .AddFloatRange(0, 120, 2.5f, 12, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => ShapeshiftCooldown = f)
                .Build())
            .SubOption(sub => sub.Name("Shapeshift Duration")
                .Value(1f)
                .AddFloatRange(2, 120, 2.5f, 6, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => ShapeshiftDuration = f)
                .Build());
}