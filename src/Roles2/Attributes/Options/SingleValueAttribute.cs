using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles2.Attributes.Options;

public class SingleValueAttribute: RoleOptionAttribute
{
    private const float CValue = 1.1f;
    private object value;
    public string? LocalizedText;
    private Color color = Color.clear;

    public SingleValueAttribute(int value, string? localizedText = null, float r = CValue, float g = CValue, float b = CValue) : base(SuffixType.None)
    {
        this.value = value;
        LocalizedText = localizedText;
        if (r < CValue || g < CValue || b < CValue) color = new Color(r, g, b);
    }

    public SingleValueAttribute(string value, string? localizedText = null, float r = CValue, float g = CValue, float b = CValue) : base(SuffixType.None)
    {
        this.value = value;
        LocalizedText = localizedText;
        if (r < CValue || g < CValue || b < CValue) color = new Color(r, g, b);
    }

    public SingleValueAttribute(float value, string? localizedText = null, float r = CValue, float g = CValue, float b = CValue) : base(SuffixType.None)
    {
        this.value = value;
        LocalizedText = localizedText;
        if (r < CValue || g < CValue || b < CValue) color = new Color(r, g, b);
    }

    public override GameOptionBuilder ConfigureBuilder(GameOptionBuilder optionBuilder, AttributeContext context)
    {
        string text = LocalizedText ?? (value as string ?? "ERROR");
        RoleLocalizedAttribute? localizedAttribute = context.Representation.LocalizedAttribute;
        if (localizedAttribute != null)
        {
            localizedAttribute = localizedAttribute.Clone();
            if (localizedAttribute.Group != null) localizedAttribute.Group += "." + localizedAttribute.Key;
            localizedAttribute.Key = (LocalizedText ?? (value as string ?? $"Value{value}")).Replace(" ", "");
            text = context.Localizer.ProvideTranslation(localizedAttribute, text);
        }

        return optionBuilder.Value(v =>
        {
            if (color != Color.clear) v = v.Color(color);
            return v.Text(text).Value(value).Build();
        });
    }
}