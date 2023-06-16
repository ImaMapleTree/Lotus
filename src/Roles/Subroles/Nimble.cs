using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Legacy;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.Subroles;

public class Nimble: Subrole
{
    protected float VentCooldown;
    protected float VentDuration;

    public override string Identifier() => "☁";

    protected override void PostSetup()
    {
        CustomRole myRole = MyPlayer.GetCustomRole();
        myRole.BaseCanVent = true;
        myRole.RoleAbilityFlags &= ~RoleAbilityFlag.CannotVent;
        if (!myRole.RealRole.IsImpostor()) myRole.VirtualRole = RoleTypes.Engineer;
        if (myRole.DesyncRole is RoleTypes.Crewmate) myRole.DesyncRole = RoleTypes.Engineer;
        Game.MatchData.Roles.AddOverride(MyPlayer.PlayerId, new GameOptionOverride(Override.EngVentCooldown, VentCooldown));
        Game.MatchData.Roles.AddOverride(MyPlayer.PlayerId, new GameOptionOverride(Override.EngVentDuration, VentDuration));
    }

    public override bool IsAssignableTo(PlayerControl player)
    {
        return player.GetVanillaRole() is RoleTypes.Crewmate && base.IsAssignableTo(player);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Key("Vent Cooldown")
                .Name(Engineer.EngineerTranslations.Options.VentCooldown)
                .AddFloatRange(0, 120, 2.5f, 16, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => VentCooldown = f)
                .Build())
            .SubOption(sub => sub.Name(Engineer.EngineerTranslations.Options.VentDuration)
                .Key("Vent Duration")
                .Value(1f)
                .AddFloatRange(2, 120, 2.5f, 6, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => VentDuration = f)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(1f, 0.98f, 0.65f));
}