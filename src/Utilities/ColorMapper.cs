using System;
using System.Collections.Generic;
using System.Linq;
using VentLib.Utilities.Attributes;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Lotus.Utilities;

[LoadStatic]
public class ColorMapper
{

    private static Regex _regex = new("(\\B[A-Z])");
    static ColorMapper()
    {
        HashSet<KnownColor> ignoredColors = new()
        {
            KnownColor.Control, KnownColor.Desktop, KnownColor.Highlight, KnownColor.Menu, KnownColor.Info,
            KnownColor.Window, KnownColor.ActiveBorder,
            KnownColor.ActiveCaption, KnownColor.AppWorkspace, KnownColor.ButtonFace, KnownColor.ButtonHighlight,
            KnownColor.ButtonShadow, KnownColor.ControlDark,
            KnownColor.ControlLight, KnownColor.ControlText, KnownColor.GrayText, KnownColor.HighlightText,
            KnownColor.HotTrack, KnownColor.InactiveBorder, KnownColor.InactiveCaption,
            KnownColor.InfoText, KnownColor.MenuBar, KnownColor.MenuHighlight, KnownColor.MenuText,
            KnownColor.RebeccaPurple, KnownColor.ScrollBar, KnownColor.WindowFrame,
            KnownColor.WindowText, KnownColor.ActiveCaptionText, KnownColor.ControlDarkDark,
            KnownColor.ControlLightLight, KnownColor.GradientActiveCaption, KnownColor.GradientInactiveCaption,
            KnownColor.InactiveCaptionText,
        };

        foreach (KnownColor kc in Enum.GetValues(typeof(KnownColor)))
        {
            if (ignoredColors.Contains(kc)) continue;
            Color c = Color.FromKnownColor(kc);
            colorMap[c.ToArgb() & 0x00FFFFFF] = c.Name;
        }
    }

    //create the dictionary with the elements you are interested in
    private static Dictionary<int, string> colorMap = new();

    public static string? GetName(Color color)
    {
        //mask out the alpha channel
        int myRgb = (int)(color.ToArgb() & 0x00FFFFFF);
        if (colorMap.ContainsKey(myRgb))
        {
            return colorMap[myRgb];
        }
        return null;
    }
    public static string GetNearestName(Color color)
    {
        //check first for an exact match
        string? name = GetName(color);
        if (name != null) return _regex.Replace(name, " $1");

        //mask out the alpha channel
        int myRgb = color.ToArgb() & 0x00FFFFFF;
        //retrieve the color from the dictionary with the closest measure
        int closestColor = colorMap.Keys.Select(colorKey => new ColorDistance(colorKey, myRgb)).MinBy(d => d.distance).colorKey;
        //return the name
        return _regex.Replace(colorMap[closestColor], " $1");
    }
}

//Just a simple utility class to store our
//color values and the distance from the color of interest
public class ColorDistance
{
    private int _colorKey;
    public int colorKey => _colorKey;
    private int _distance;
    public int distance => _distance;

    public ColorDistance(int colorKeyRgb, int rgb2)
    {
        //store for use at end of query
        this._colorKey = colorKeyRgb;

        //we just pull the individual color components out
        byte r1 = (byte)((colorKeyRgb >> 16) & 0xff);
        byte g1 = (byte)((colorKeyRgb >> 8) & 0xff);
        byte b1 = (byte)((colorKeyRgb) & 0xff);

        byte r2 = (byte)((rgb2 >> 16) & 0xff);
        byte g2 = (byte)((rgb2 >> 8) & 0xff);
        byte b2 = (byte)((rgb2) & 0xff);

        //provide a simple distance measure between colors
        _distance = Math.Abs(r1 - r2) + Math.Abs(g1 - g2) + Math.Abs(b1 - b2);
    }
}