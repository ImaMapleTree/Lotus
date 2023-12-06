extern alias JBAnnotations;
using JBAnnotations::JetBrains.Annotations;
using VentLib.Options.Game;

namespace Lotus.Roles2.Attributes.Options;

[UsedImplicitly]
public class FloatRangeOptionAttribute: RoleOptionAttribute
{
    private float min;
    private float max;
    private float step;
    private int defaultIndex;

    public FloatRangeOptionAttribute(float min, float max, float step, int defaultIndex = 0, SuffixType suffixType = SuffixType.None) : base(suffixType)
    {
        this.min = min;
        this.max = max;
        this.step = step;
        this.defaultIndex = defaultIndex;
    }

    public FloatRangeOptionAttribute(float min, float max, float step, string suffix, int defaultIndex = 0) : base(SuffixType.Custom, suffix)
    {
        this.min = min;
        this.max = max;
        this.step = step;
        this.defaultIndex = defaultIndex;
    }

    public override GameOptionBuilder ConfigureBuilder(GameOptionBuilder optionBuilder, AttributeContext context)
    {
        return optionBuilder.AddFloatRange(min, max, step, defaultIndex, Suffix);
    }
}