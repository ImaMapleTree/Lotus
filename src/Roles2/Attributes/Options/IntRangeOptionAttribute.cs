extern alias JBAnnotations;
using JBAnnotations::JetBrains.Annotations;
using VentLib.Options.Game;

namespace Lotus.Roles2.Attributes.Options;

[UsedImplicitly]
public class IntRangeOptionAttribute: RoleOptionAttribute
{
    private readonly int min;
    private readonly int max;
    private readonly int step;
    private readonly int defaultIndex;

    public IntRangeOptionAttribute(int min, int max, int step, int defaultIndex = 0, SuffixType suffixType = SuffixType.None) : base(suffixType)
    {
        this.min = min;
        this.max = max;
        this.step = step;
        this.defaultIndex = defaultIndex;
    }

    public IntRangeOptionAttribute(int min, int max, int step, string suffix, int defaultIndex = 0) : base(SuffixType.Custom, suffix)
    {
        this.min = min;
        this.max = max;
        this.step = step;
        this.defaultIndex = defaultIndex;
    }

    public override GameOptionBuilder ConfigureBuilder(GameOptionBuilder optionBuilder, AttributeContext context)
    {
        return optionBuilder.AddIntRange(min, max, step, defaultIndex, Suffix);
    }
}