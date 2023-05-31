using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;

namespace Lotus.Roles.Subroles;

public class Deadly: Subrole
{
    private int cooldownReduction;
    private Remote<GameOptionOverride>? overrideRemote;

    public override string Identifier() => "乂";

    [RoleAction(RoleActionType.RoundStart)]
    private void GameStart(bool isStart)
    {
        if (!isStart) return;
        MultiplicativeOverride multiplicativeOverride = new(Override.KillCooldown, (100f - cooldownReduction) / 100f);
        overrideRemote = Game.MatchData.Roles.AddOverride(MyPlayer.PlayerId, multiplicativeOverride);
    }

    public override bool IsAssignableTo(PlayerControl player)
    {
        return player.GetVanillaRole().IsImpostor() && base.IsAssignableTo(player);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Cooldown Reduction", Translations.Options.CooldownReduction)
                .AddIntRange(0, 100, 5, 5, "%")
                .BindInt(i => cooldownReduction = i)
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.45f, 0.64f, 0.4f));

    [Localized(nameof(Deadly))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            public static string CooldownReduction = "Cooldown Reduction";
        }
    }
}