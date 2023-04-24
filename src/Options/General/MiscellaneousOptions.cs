using System.Collections.Generic;
using TOHTOR.Extensions;
using TOHTOR.Roles;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace TOHTOR.Options.General;

[Localized("Options")]
public class MiscellaneousOptions
{
    private static Color _optionColor = new(1f, 0.75f, 0.81f);
    private static List<GameOption> additionalOptions = new();

    public string AssignedPet = null!;
    public bool CosmeticCommands;
    public bool AutoDisplayResults;
    public int SuffixMode;
    public bool ColorNameMode;
    public int LadderDeathChance = -1;

    public bool EnableLadderDeath => LadderDeathChance > 0;

    public MiscellaneousOptions()
    {

        new GameOptionTitleBuilder()
            .Title(MiscOptionTranslations.MiscOptionTitle)
            .Color(_optionColor)
            .Tab(DefaultTabs.GeneralTab)
            .Build();

        GameOptionBuilder AddPets(GameOptionBuilder b)
        {
            foreach ((string? key, string? value) in ModConstants.Pets) b = b.Value(v => v.Text(key).Value(value).Build());
            return b;
        }

        AddPets(Builder("Assigned Pet")
                .Name(MiscOptionTranslations.AssignedPetText)
                .IsHeader(true)
                .Tab(DefaultTabs.GeneralTab)
                .BindString(s => AssignedPet = s))
            .BuildAndRegister();

        var cosmeticCommands = Builder("Allow Cosmetic Commands")
            .Name(MiscOptionTranslations.CosmeticCommandText)
            .AddOnOffValues(false)
            .BindBool(b => CosmeticCommands = b)
            .BuildAndRegister();

        var autoDisplayResults = Builder("Auto Display Results")
            .Name(MiscOptionTranslations.AutoDisplayResultsText)
            .AddOnOffValues()
            .BindBool(b => AutoDisplayResults = b)
            .BuildAndRegister();

        /*var suffixMode = Builder("Suffix Mode")*/

        var colorNameMode = Builder("Color Names")
            .Name(MiscOptionTranslations.ColorNames)
            .AddOnOffValues(false)
            .BindBool(b => ColorNameMode = b)
            .BuildAndRegister();

        var ladderDeath = Builder("Ladder Death")
            .Name(MiscOptionTranslations.LadderDeathText)
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(-1).Color(Color.red).Build())
            .AddIntRange(10, 100, 5, suffix: "%")
            .BindInt(i => LadderDeathChance = i)
            .BuildAndRegister();

        additionalOptions.ForEach(o => o.Register());
    }

    /// <summary>
    /// Adds additional options to be registered when this group of options is loaded. This is mostly used for ordering
    /// in the main menu, as options passed in here will be rendered along with this group.
    /// </summary>
    /// <param name="option">Option to render</param>
    public static void AddAdditionalOption(GameOption option)
    {
        additionalOptions.Add(option);
    }

    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.GeneralTab).Color(_optionColor);

    [Localized("Miscellaneous")]
    private static class MiscOptionTranslations
    {
        [Localized("SectionTitle")]
        public static string MiscOptionTitle = "Miscellaneous Options";

        [Localized("AssignedPet")]
        public static string AssignedPetText = "Assigned Pet";

        [Localized(nameof(CosmeticCommandText))]
        public static string CosmeticCommandText = "Allow /name, /color, and /level";

        [Localized("AutoDisplayResults")]
        public static string AutoDisplayResultsText = "Auto Display Results";

        [Localized("SuffixMode")]
        public static string SuffixModeText = "Suffix Mode";

        [Localized(nameof(ColorNames))]
        public static string ColorNames = "Color Names";

        [Localized("LadderDeath")]
        public static string LadderDeathText = "Ladder Death";
    }

}