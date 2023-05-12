using System.Linq;
using VentLib.Options;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Chat;

public class OptionUtils
{
    public static string OptionText(Option opt, int level = 0)
    {
        string levelText = level == 0 ? "" : "   ".Repeat(level - 1);
        string text = $"{levelText}• {opt.Name()}: {opt.GetValueText()}\n";
        return text + opt.Children.GetConditionally(opt.GetValue()).Select(o => OptionText(o, level + 1)).Fuse("");
    }
}