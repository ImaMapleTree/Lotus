using Lotus.Extensions;
using Lotus.Managers;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options;
using VentLib.Utilities;

namespace Lotus.Options.Roles;

[Localized(ModConstants.Options)]
public class CrewmateOptions
{
    public CrewmateOptions()
    {
        OptionManager optionManager = OptionManager.GetManager(file: "options.txt");

        CustomRoleManager.Special.CrewGuesser.GetGameOptionBuilder()
            .Tab(DefaultTabs.CrewmateTab)
            .KeyName("Crewmate Guessers", Color.white.Colorize(TranslationUtil.Colorize(Translations.CrewmateGuessers, ModConstants.Palette.CrewmateColor)))
            .BuildAndRegister(optionManager);
    }

    [Localized("RolesCrewmates")]
    private static class Translations
    {
        [Localized(nameof(CrewmateGuessers))]
        public static string CrewmateGuessers = "Crewmates::0 Guessers";
    }
}