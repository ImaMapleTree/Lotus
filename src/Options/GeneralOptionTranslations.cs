using VentLib.Localization.Attributes;

namespace TOHTOR.Options;

[Localized("Options.General")]
public static class GeneralOptionTranslations
{
    [Localized(nameof(OnText))]
    public static string OnText = "ON";

    [Localized(nameof(OffText))]
    public static string OffText = "OFF";

    [Localized(nameof(EnabledText))]
    public static string EnabledText = "Enabled";

    [Localized(nameof(DisabledText))]
    public static string DisabledText = "Disabled";

    [Localized(nameof(DefaultText))]
    public static string DefaultText = "Default";

    [Localized(nameof(NeverText))]
    public static string NeverText = "Never";

    [Localized(nameof(ShowText))]
    public static string ShowText = "Show";

    [Localized(nameof(HideText))]
    public static string HideText = "Hide";

    [Localized(nameof(FriendsText))]
    public static string FriendsText = "Friends";

    [Localized(nameof(EveryoneText))]
    public static string EveryoneText = "Everyone";

    [Localized(nameof(DurationText))]
    public static string DurationText = "Duration";

    [Localized(nameof(AllText))]
    public static string AllText = "All";
}