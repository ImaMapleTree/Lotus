using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Utilities;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace Lotus.Options.Roles;

[Localized(ModConstants.Options)]
public class MadmateOptions
{
    public bool MadmatesTakeImpostorSlots;
    public int MinimumMadmates;
    public int MaximumMadmates;

    string GColor(string input) => TranslationUtil.Colorize(input, ModConstants.Palette.MadmateColor);

    public MadmateOptions()
    {
        new GameOptionTitleBuilder().Title($"<size=2.3>★ {FactionInstances.Madmates.Name()} ★</size>")
            .Color(ModConstants.Palette.MadmateColor)
            .Tab(DefaultTabs.ImpostorsTab)
            .Build();

        Builder("Madmates Take Impostor Slots")
            .Name(GColor(Translations.MadmatesTakeImpostorSlots))
            .IsHeader(true)
            .AddOnOffValues()
            .BindBool(b => MadmatesTakeImpostorSlots = b)
            .ShowSubOptionPredicate(b => !(bool)b)
            .SubOption(sub2 => sub2.KeyName("Minimum Madmates", GColor(Translations.MinimumMadmates))
                .AddIntRange(0, 15, 1)
                .BindInt(i => MinimumMadmates = i)
                .Build())
            .BuildAndRegister();

        Builder("Maximum Madmates")
            .Name(GColor(Translations.MaximumMadmates))
            .AddIntRange(0, 15, 1)
            .BindInt(i => MaximumMadmates = i)
            .BuildAndRegister();
    }

    private static GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.ImpostorsTab);

    [Localized("RolesMadmates")]
    private static class Translations
    {
        [Localized(nameof(MadmatesTakeImpostorSlots))]
        public static string MadmatesTakeImpostorSlots = "Madmates::0 Take Impostor Slots";

        public static string MinimumMadmates = "Minimum Madmates::0";

        public static string MaximumMadmates = "Maximum Madmates::0";
    }
}