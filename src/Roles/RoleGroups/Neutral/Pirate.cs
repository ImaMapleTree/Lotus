using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.RoleGroups.Stock;
using Lotus.Roles.Subroles;
using Lotus.Utilities;
using Lotus.Victory.Conditions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Pirate: GuesserRoleBase
{
    private int pirateGuessesToWin;
    private bool pirateDiesOnMissguess;

    [UIComponent(UI.Counter, ViewMode.Additive, GameState.Roaming, GameState.InMeeting)]
    public string ShowGuessTotal() => RoleUtils.Counter(CorrectGuesses, pirateGuessesToWin, RoleColor);

    [RoleAction(RoleActionType.RoundStart)]
    public void RoundStartCheckWinCondition()
    {
        if (pirateGuessesToWin != CorrectGuesses) return;
        ManualWin.Activate(MyPlayer, ReasonType.RoleSpecificWin, 999);
    }

    protected override void HandleBadGuess()
    {
        if (!pirateDiesOnMissguess) return;
        base.HandleBadGuess();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Pirate Guess Win Amount", TranslationUtil.Colorize(Translations.Options.PirateGuesses, RoleColor))
                .AddIntRange(0, 15, 1, 3)
                .BindInt(i => pirateGuessesToWin = i)
                .Build())
            .SubOption(sub => sub.KeyName("Pirate Dies on Missguess", TranslationUtil.Colorize(Translations.Options.PirateDiesOnMissGues, RoleColor))
                .BindBool(b => pirateDiesOnMissguess = b)
                .AddOnOffValues()
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(0.93f, 0.76f, 0.25f))
            .Faction(FactionInstances.Neutral)
            .SpecialType(SpecialType.Neutral);


    [Localized(nameof(Pirate))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            public static string PirateGuesses = "Pirate::0 Guess Win Amount";

            public static string PirateDiesOnMissGues = "Pirate::0 Dies on Missguess";
        }
    }
}