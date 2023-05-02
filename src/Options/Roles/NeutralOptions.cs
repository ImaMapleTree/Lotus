using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace TOHTOR.Options.Roles;

[Localized("Options")]
public class NeutralOptions
{
    public static Color NeutralColor = new(1f, 0.67f, 0.11f);
    public static Color PassiveColor = new(1f, 0.87f, 0.91f);
    public static Color KillingColor = new(1f, 0.27f, 0.18f);

    public int NeutralPassiveSlots;
    public int NeutralKillingSlots;

    public NeutralOptions()
    {
        string GColor(string input) => TranslationUtil.Colorize(input, NeutralColor, PassiveColor, KillingColor);

        Builder("Neutral Passive Slots")
            .IsHeader(true)
            .Name(GColor(NeutralOptionTranslations.NeutralPassiveSlots))
            .BindInt(i => NeutralPassiveSlots = i)
            .BuildAndRegister();

        Builder("Neutral Killing Slots")
            .Name(GColor(NeutralOptionTranslations.NeutralKillingSlots))
            .BindInt(i => NeutralKillingSlots = i)
            .BuildAndRegister();
    }

    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.NeutralTab).AddIntRange(0, 15);

    [Localized("RolesNeutral")]
    private static class NeutralOptionTranslations
    {
        [Localized(nameof(NeutralPassiveSlots))]
        public static string NeutralPassiveSlots = "Neutral::0 Passive::1 Slots";

        [Localized(nameof(NeutralKillingSlots))]
        public static string NeutralKillingSlots = "Neutral::0 Killing::2 Slots";

    }
}