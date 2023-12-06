using System;

namespace Lotus.Roles2;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
public class RoleLocalizedAttribute: Attribute
{
    public string? Translation;
    public string? Group;
    public string? Key;

    public RoleLocalizedAttribute(string? translation = null, string? key = null, string? group = null)
    {
        this.Translation = translation;
        this.Key = key;
        this.Group = group;
    }

    public RoleLocalizedAttribute Clone() => (RoleLocalizedAttribute)this.MemberwiseClone();
}