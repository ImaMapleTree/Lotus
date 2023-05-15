using Lotus.Options;
using VentLib.Options;
using VentLib.Options.Game;

namespace Lotus.Extensions;

public static class GameOptionBuilderExtensions
{
    public static GameOptionBuilder AddOnOffValues(this GameOptionBuilder builder, bool defaultOn = true)
    {
        return builder.Value(val =>
                    val.Text(defaultOn ? GeneralOptionTranslations.OnText : GeneralOptionTranslations.OffText)
                        .Value(defaultOn)
                        .Color(defaultOn ? UnityEngine.Color.cyan : UnityEngine.Color.red)
                        .Build())
                .Value(val =>
                    val.Text(defaultOn ? GeneralOptionTranslations.OffText : GeneralOptionTranslations.OnText)
                        .Value(!defaultOn)
                        .Color(defaultOn ? UnityEngine.Color.red : UnityEngine.Color.cyan)
                        .Build());
    }

    public static GameOptionBuilder AddEnableDisabledValues(this GameOptionBuilder builder, bool defaultOn = true)
    {
        return builder.Value(val =>
                val.Text(defaultOn ? GeneralOptionTranslations.EnabledText : GeneralOptionTranslations.DisabledText)
                    .Value(defaultOn)
                    .Color(defaultOn ? UnityEngine.Color.cyan : UnityEngine.Color.red)
                    .Build())
            .Value(val =>
                val.Text(defaultOn ? GeneralOptionTranslations.DisabledText : GeneralOptionTranslations.EnabledText)
                    .Value(!defaultOn)
                    .Color(defaultOn ? UnityEngine.Color.red : UnityEngine.Color.cyan)
                    .Build());
    }

    public static GameOptionBuilder KeyName(this GameOptionBuilder builder, string key, string name) => builder.Key(key).Name(name);
}