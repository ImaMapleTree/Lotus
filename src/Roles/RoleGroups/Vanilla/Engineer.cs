using AmongUs.GameOptions;
using Lotus.Roles.Overrides;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Engineer: Crewmate
{
    protected float VentCooldown;
    protected float VentDuration;
    
    protected GameOptionBuilder AddVentingOptions(GameOptionBuilder builder)
    {
        return builder.SubOption(sub => sub.Name("Vent Cooldown")
                .AddFloatRange(0, 120, 2.5f, 16, "s")
                .BindFloat(f => VentCooldown = f)
                .Build())
            .SubOption(sub => sub.Name("Vent Duration")
                .Value(1f)
                .AddFloatRange(2, 120, 2.5f, 6, "s")
                .BindFloat(f => VentDuration = f)
                .Build());
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .CanVent(true)
            .VanillaRole(RoleTypes.Engineer)
            .OptionOverride(Override.EngVentCooldown, VentCooldown)
            .OptionOverride(Override.EngVentDuration, VentDuration);
}