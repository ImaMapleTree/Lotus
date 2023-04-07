using VentLib.Options.Game;

namespace TOHTOR.Roles.RoleGroups.Vanilla;

public class Morphling : Shapeshifter
{
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Shapeshift Cooldown")
                .AddFloatRange(0, 120, 2.5f, 12, "s")
                .BindFloat(f => ShapeshiftCooldown = f)
                .Build())
            .SubOption(sub => sub.Name("Shapeshift Duration")
                .AddFloatRange(2, 120, 2.5f, 6, "s")
                .BindFloat(f => ShapeshiftDuration = f)
                .Build());
}