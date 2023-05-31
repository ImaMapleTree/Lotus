using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Managers.History.Events;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;

namespace Lotus.Roles.Subroles;

public class Escalation : Subrole
{
    private float speedGainPerKill;
    private int kills;

    private Remote<GameOptionOverride>? remote;

    public override string Identifier() => "◉";

    protected override void PostSetup()
    {
        AdditiveOverride additiveOverride = new(Override.PlayerSpeedMod, () => kills * speedGainPerKill);
        remote = Game.MatchData.Roles.AddOverride(MyPlayer.PlayerId, additiveOverride);
    }

    [RoleAction(RoleActionType.AnyDeath)]
    public void CheckPlayerDeath(PlayerControl target, PlayerControl killer, IDeathEvent deathEvent)
    {
        if (target.PlayerId == MyPlayer.PlayerId) remote?.Delete();
        killer = deathEvent.Instigator().OrElse(killer);
        if (killer.PlayerId != MyPlayer.PlayerId) return;

        kills++;
        killer.GetCustomRole().SyncOptions();
    }

    public override bool IsAssignableTo(PlayerControl player)
    {
        if (!(player.GetVanillaRole().IsImpostor() || player.GetCustomRole().RoleAbilityFlags.HasFlag(RoleAbilityFlag.IsAbleToKill))) return false;
        return base.IsAssignableTo(player);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Additional Speed per Kill", Translations.Options.SpeedGainPerKill)
                .AddFloatRange(0.1f, 1f, 0.1f, 3)
                .BindFloat(f => speedGainPerKill = f)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleFlags(RoleFlag.VariationRole)
            .RoleColor(new Color(0.78f, 0.62f, 0.04f));

    [Localized(nameof(Escalation))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(SpeedGainPerKill))]
            public static string SpeedGainPerKill = "Additional Speed per Kill";
        }
    }
}