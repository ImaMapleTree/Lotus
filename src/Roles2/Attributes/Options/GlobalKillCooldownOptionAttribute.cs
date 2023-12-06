using Lotus.Options;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles2.Attributes.Options;

[RoleLocalized(key: "Kill Cooldown", translation: "Kill Cooldown")]
public class GlobalKillCooldownOptionAttribute: FloatRangeOptionAttribute
{
    public GlobalKillCooldownOptionAttribute() : base(0, 120, 2.5f, 0, SuffixType.Seconds)
    {
    }

    public GlobalKillCooldownOptionAttribute(float min, float max = 120, float step = 2.5f, int defaultIndex = 0): base(min, max, step, defaultIndex, SuffixType.Seconds)
    {
    }


    public override GameOptionBuilder ConfigureBuilder(GameOptionBuilder optionBuilder, AttributeContext context)
    {
        return base.ConfigureBuilder(optionBuilder.Value(v => v.Text(GeneralOptionTranslations.GlobalText).Color(new Color(1f, 0.61f, 0.33f)).Value(-1f).Build()), context);
    }
}