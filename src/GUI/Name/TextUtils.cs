using System;
using System.Text.RegularExpressions;
using UnityEngine;
using VentLib.Utilities;

namespace TOHTOR.GUI.Name;

public class TextUtils
{
    private static Regex utf32Regex = new(@"\\u[a-zA-Z0-9]*");

    public static string ApplySize(float size, string str)
    {
        return $"<size={size}>{str}</size>";
    }

    public string ToUTF32(string input)
    {
        string output = input;

        while (output.Contains(@"\u"))
        {
            output = utf32Regex.Replace(output, @"\U000" + output.Substring(output.IndexOf(@"\u", StringComparison.Ordinal) + 2, 5), 1);
        }

        return output;
    }

    public static string ApplyGradient(string str, params Color[] colors)
    {
        return new ColorGradient(colors).Apply(str);
    }
}