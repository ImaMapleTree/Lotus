using TOHTOR.Extensions;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace TOHTOR.Options.Roles;

[Localized("Options")]
public class SubroleOptions
{
    public static Color ModifierColor = new(0.43f, 0.89f, 0.61f);
    
    public int ModifierLimits;
    public bool EvenlyDistributeModifiers;
    public bool UncappedModifiers => ModifierLimits == -1;
    
    
    public SubroleOptions()
    {
        Builder("Maximum Modifiers per Player")
            .Name(TranslationUtil.Colorize(SubroleOptionTranslations.ModifierMaximumText, ModifierColor))
            .IsHeader(true)
            .Value(v => v.Text("Uncapped").Color(new Color(0.19f, 0.8f, 0f)).Value(-1).Build())
            .AddIntRange(1, 10, 1)
            .BindInt(i => ModifierLimits = i)
            .BuildAndRegister();

        Builder("Evenly Distribute Modifiers")
            .Name(TranslationUtil.Colorize(SubroleOptionTranslations.EvenlyDistributeModifierText, ModifierColor))
            .AddOnOffValues()
            .BindBool(b => EvenlyDistributeModifiers = b)
            .BuildAndRegister();

    }
    
    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.MiscTab);

    [Localized("Subroles")]
    private static class SubroleOptionTranslations
    {
        [Localized(nameof(ModifierMaximumText))]
        public static string ModifierMaximumText = "Maximum Modifiers::0 per Player";

        [Localized(nameof(EvenlyDistributeModifierText))]
        public static string EvenlyDistributeModifierText = "Evenly Distribute Modifiers::0";
    }
}