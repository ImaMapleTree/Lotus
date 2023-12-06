using System;
using Lotus.Options;

namespace Lotus.Roles2.Attributes.Options;

public enum SuffixType
{
    None,
    Custom,
    Seconds,
    Multiplier,
    Percentage
}

public static class SuffixTypeMethods
{
    public static string GetSuffix(this SuffixType suffixType)
    {
        return suffixType switch
        {
            SuffixType.None => "",
            SuffixType.Custom => "",
            SuffixType.Seconds => GeneralOptionTranslations.SecondsSuffix,
            SuffixType.Multiplier => "x",
            SuffixType.Percentage => "%",
            _ => throw new ArgumentOutOfRangeException(nameof(suffixType), suffixType, null)
        };
    }
}