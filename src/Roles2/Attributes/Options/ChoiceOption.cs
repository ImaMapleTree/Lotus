using System;
using System.Collections.Generic;
using Lotus.Utilities;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace Lotus.Roles2.Attributes.Options;

public class ChoiceOption: RoleOptionAttribute
{
    public int DefaultIndex { get; }

    public ChoiceOption(string suffix, int defaultIndex = 0) : base(SuffixType.Custom, suffix)
    {
        this.DefaultIndex = defaultIndex;
    }

    public ChoiceOption(SuffixType suffixType, int defaultIndex = 0) : base(suffixType)
    {
        this.DefaultIndex = defaultIndex;
    }

    public ChoiceOption(int defaultIndex) : base(SuffixType.None)
    {
        this.DefaultIndex = defaultIndex;
    }

    public ChoiceOption(): base(SuffixType.None) {}

    public override GameOptionBuilder ConfigureBuilder(GameOptionBuilder optionBuilder, AttributeContext context)
    {
        List<Option> choices = context.Reflector.GetAttributes<Option>();
        for (int index = 0; index < choices.Count; index++)
        {
            Option c = choices[index];
            int index1 = index;

            optionBuilder.Value(vb => ValueFunc(vb).Build());
            continue;

            GameOptionValueBuilder ValueFunc(GameOptionValueBuilder vb)
            {
                vb = vb.Value(c.Value).Suffix(Suffix);
                if (c.LocalizedText == null || context.Representation.LocalizedAttribute == null) return vb;

                RoleLocalizedAttribute clonedAttribute = CloneUtils.Clone(context.Representation.LocalizedAttribute);
                clonedAttribute.Translation = c.LocalizedText;
                clonedAttribute.Key += ".Choice" + index1;
                vb = vb.Text(context.Localizer.ProvideTranslation(clonedAttribute));

                return vb;
            }
        }

        // Hack to set default index
        return optionBuilder.Values(DefaultIndex);
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public abstract class Option : Attribute
    {
        public object Value { get; }
        public string? LocalizedText { get; }

        protected Option(object value, string? localizedText)
        {
            Value = value;
            LocalizedText = localizedText;
        }
    }

    public class Int : Option
    {
        public Int(int value, string? localizedText = null) : base(value, localizedText)
        {
        }
    }

    public class Float : Option
    {
        public Float(float value, string? localizedText = null) : base(value, localizedText)
        {
        }
    }

    public class Text : Option
    {
        public Text(string value, string? localizedText = null) : base(value, localizedText)
        {
        }
    }
}