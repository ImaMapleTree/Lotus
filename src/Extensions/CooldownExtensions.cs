using Lotus.GUI;
using Lotus.Options;
using VentLib.Utilities.Extensions;

namespace Lotus.Extensions;

public static class CooldownExtensions
{
    public static string Format(this Cooldown cooldown, string str, bool autoFormat = false)
    {
        if (cooldown.IsReady()) return "";
        return autoFormat ? str.Formatted(cooldown + GeneralOptionTranslations.SecondsSuffix) : str;
    }
}