using Lotus.Extensions;
using Lotus.Managers;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options;
using VentLib.Utilities;

namespace Lotus.Options.Roles;

public class ImpostorOptions
{
    public ImpostorOptions()
    {
        OptionManager optionManager = OptionManager.GetManager(file: "options.txt");

        CustomRoleManager.Special.ImpGuesser.GetGameOptionBuilder()
            .Tab(DefaultTabs.ImpostorsTab)
            .KeyName("Impostor Guessers", Color.white.Colorize(TranslationUtil.Colorize(Translations.ImpostorGuessers, Color.red)))
            .BuildAndRegister(optionManager);
    }

    [Localized("RolesImpostors")]
    private static class Translations
    {
        [Localized(nameof(ImpostorGuessers))]
        public static string ImpostorGuessers = "Impostor::0 Guessers";
    }

}