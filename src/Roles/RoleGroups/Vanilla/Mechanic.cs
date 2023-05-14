using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Mechanic : Engineer
{
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Vent Cooldown")
                .AddFloatRange(0, 120, 2.5f, 16, "s")
                .BindFloat(f => VentCooldown = f)
                .Build())
            .SubOption(sub => sub.Name("Vent Duration")
                .Value(1f)
                .AddFloatRange(2, 120, 2.5f, 6, "s")
                .BindFloat(f => VentDuration = f)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(1f, 0.65f, 0.04f));
}