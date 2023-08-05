// ReSharper disable InconsistentNaming

using System;
using System.Text.RegularExpressions;
using Lotus.Logging;
using Lotus.Managers.Templates.Models.Units;
using Lotus.Utilities;

namespace Lotus.Managers.Templates.Models.Backing;

internal class InlineConditionEvaluator
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(InlineConditionEvaluator));

    private const string EQ = "==";
    private const string CEQ = "===";
    private const string NE = "!=";
    private const string CNE = "!==";
    private const string GT = ">";
    private const string LT = "<";
    private const string GTE = ">=";
    private const string LTE = "<=";
    private const string CTN = "[Contains]";
    private const string DCTN = "[!Contains]";
    private const string MCH = "[Matches]";
    private const string DMCH = "[!Matches]";

    private static Regex _regex = new($@"(.*?)\s*({CEQ}|{CNE}|{EQ}|{NE}|{GTE}|{LTE}|{LT}|{GT}|\{CTN}|\{DCTN}|\{DMCH}|\{MCH})\s*(.*)");


    public static bool Evaluate(string inlineCondition, object? obj)
    {
        var groups = _regex.Match(inlineCondition).Groups;

        if (!ExtractGroupValue(groups, 1, out string left)) return true;
        if (!ExtractGroupValue(groups, 2, out string middle)) return true;
        if (!ExtractGroupValue(groups, 3, out string right)) return true;

        left = left.RemoveHtmlTags();
        left = TemplateUnit.FormatStatic(left, obj);
        left = left.RemoveHtmlTags();

        right = right.RemoveHtmlTags();
        right = TemplateUnit.FormatStatic(right, obj);
        right = right.RemoveHtmlTags();

        DevLogger.Log($"Left: {left}");
        DevLogger.Log($"right: {right}");

        switch (middle)
        {
            case EQ:
                return string.Equals(left, right, StringComparison.CurrentCultureIgnoreCase);
            case CEQ:
                return left == right;
            case NE:
                return !string.Equals(left, right, StringComparison.CurrentCultureIgnoreCase);
            case CNE:
                return left != right;
            case CTN:
                return left.ToLower().Contains(right.ToLower());
            case DCTN:
                return !left.ToLower().Contains(right.ToLower());
            case MCH:
                return Regex.IsMatch(left, right, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            case DMCH:
                return !Regex.IsMatch(left, right, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            case GT:
            case GTE:
            case LT:
            case LTE:
                break;
            default:
                log.Warn($"Operator \"{middle}\" is not supported in Conditional Evaluate statements.");
                return true;
        }

        if (!ParseFloatValue(left, out float lValue)) return true;
        if (!ParseFloatValue(right, out float rValue)) return true;

        return middle switch
        {
            GT => lValue > rValue,
            GTE => lValue >= rValue,
            LT => lValue < rValue,
            LTE => lValue <= rValue,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static bool ParseFloatValue(string input, out float output)
    {
        output = 0;
        try
        {
            output = float.Parse(input);
            return true;
        }
        catch (Exception exception)
        {
            log.Exception($"Could not parse \"{input}\" to float.", exception);
            return false;
        }
    }

    private static bool ExtractGroupValue(GroupCollection input, int index, out string output)
    {
        output = "";
        try
        {
            output = input[index].Value;
            return true;
        }
        catch (Exception exception)
        {
            log.Exception("Could not parse \"Evaluate\" condition", exception);
            return false;
        }
    }

}