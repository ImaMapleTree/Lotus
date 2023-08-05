using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using VentLib.Utilities;

namespace Lotus.Utilities;

public class TranslationUtil
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(TranslationUtil));

    private static Regex taggedStringRegex = new("(\\S*::\\d*)");


    public static string Colorize(string input, params Color[] colors)
    {

        try
        {
            string[] tagStrings = taggedStringRegex.Matches(input).Select(m => m.Value).ToArray();

            string[] replacements = tagStrings.Select(v => v.Split("::"))
                .Select(va => colors[int.Parse(va[1])].Colorize(va[0])).ToArray();

            for (int index = 0; index < tagStrings.Length; index++)
            {
                string tagString = tagStrings[index];
                input = input.Replace(tagString, replacements[index]);
            }

        }
        catch (Exception exception)
        {
            log.Exception("Error colorizing message!", exception);
        }

        return input;
    }
}