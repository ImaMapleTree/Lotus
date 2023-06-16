using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Managers;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Subroles;

public class Oblivious: Subrole
{
    public override string Identifier() => "⁈";

    private bool passOnDeath;

    [RoleAction(RoleActionType.MyDeath)]
    private void ObliviousDies(PlayerControl killer, Optional<FrozenPlayer> realKiller)
    {
        if (!passOnDeath) return;
        killer = realKiller.FlatMap(k => new UnityOptional<PlayerControl>(k.MyPlayer)).OrElse(killer);
        MatchData.AssignSubrole(killer, CustomRoleManager.Mods.Oblivious);
    }

    [RoleAction(RoleActionType.SelfReportBody, priority: Priority.VeryLow)]
    private void CancelReportBody(ActionHandle handle) => handle.Cancel();

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Pass on Death", Translations.Options.PassOnDeath)
                .AddOnOffValues(false)
                .BindBool(b => passOnDeath = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.49f, 0.28f, 0.5f));

    [Localized(nameof(Oblivious))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(PassOnDeath))]
            public static string PassOnDeath = "Pass on Death";
        }
    }
}
