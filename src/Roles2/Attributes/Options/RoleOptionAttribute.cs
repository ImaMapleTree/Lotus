using System;
using VentLib.Options.Game;

namespace Lotus.Roles2.Attributes.Options;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public abstract class RoleOptionAttribute: Attribute
{
    public string Suffix { get; }

    public RoleOptionAttribute(SuffixType suffixType, string suffix)
    {
        this.Suffix = suffixType is SuffixType.Custom ? suffix : suffixType.GetSuffix();
    }

    public RoleOptionAttribute(SuffixType suffixType)
    {
        this.Suffix = suffixType.GetSuffix();
    }

    public abstract GameOptionBuilder ConfigureBuilder(GameOptionBuilder optionBuilder, AttributeContext context);
}