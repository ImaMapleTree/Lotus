using Lotus.Extensions;
using VentLib.Options.Game;

namespace Lotus.Roles2.Attributes.Options;

public class BoolOptionAttribute: RoleOptionAttribute
{
    private bool defaultValue;
    private BoolOptionType boolOptionType;

    public BoolOptionAttribute(bool defaultValue = true, BoolOptionType boolOptionType = BoolOptionType.OnAndOff) : base(SuffixType.None)
    {
        this.defaultValue = defaultValue;
        this.boolOptionType = boolOptionType;
    }

    public override GameOptionBuilder ConfigureBuilder(GameOptionBuilder optionBuilder, AttributeContext context)
    {
        if (boolOptionType is BoolOptionType.OnAndOff) optionBuilder.AddOnOffValues(defaultValue);
        else optionBuilder.AddEnableDisabledValues(defaultValue);
        return optionBuilder;
    }
}

public enum BoolOptionType
{
    OnAndOff,
    EnabledAndDisabled
}