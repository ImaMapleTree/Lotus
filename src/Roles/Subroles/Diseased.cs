using Lotus.API.Odyssey;
using Lotus.Logging;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace Lotus.Roles.Subroles;

public class Diseased: Subrole
{
    private int cooldownIncrease;

    public override string Identifier() => "★";

    [RoleAction(RoleActionType.MyDeath)]
    private void DiseasedDies(PlayerControl killer)
    {
        float multiplier = (cooldownIncrease / 100f) + 1f;
        Game.MatchData.Roles.AddOverride(killer.PlayerId, new MultiplicativeOverride(Override.KillCooldown, multiplier));
        killer.GetCustomRole().SyncOptions();
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.42f, 0.4f, 0.16f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddRestrictToCrew(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.KeyName("Cooldown Increase", Translations.Options.CooldownIncrease)
                .AddIntRange(0, 100, 5, 20, "%")
                .BindInt(i => cooldownIncrease = i)
                .Build());

    [Localized(nameof(Diseased))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(CooldownIncrease))]
            public static string CooldownIncrease = "Cooldown Increase";
        }
    }
}