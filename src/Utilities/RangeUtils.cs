using System.Text.RegularExpressions;

namespace Lotus.Utilities;

public class RangeUtils
{
    private static Regex _rangeRegex = new Regex(@"(-?\d+)\.\.(-?\d+)");

    public static bool TryParseRange(string input, out (int left, int right) range)
    {
        range = (0, 0);
        Match match = _rangeRegex.Match(input);
        if (!match.Success) return false;
        if (!int.TryParse(match.Groups[1].Value, out int l)) return false;
        if (!int.TryParse(match.Groups[2].Value, out int r)) return false;
        if (l > r) (l, r) = (r, l);
        range = (l, r);
        return true;
    }
}