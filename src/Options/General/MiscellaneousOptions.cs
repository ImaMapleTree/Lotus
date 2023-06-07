using System.Collections.Generic;
using Lotus.Extensions;
using Lotus.Roles;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Options.IO;

namespace Lotus.Options.General;

[Localized(ModConstants.Options)]
public class MiscellaneousOptions
{
    private static Color _optionColor = new(1f, 0.75f, 0.81f);
    private static List<GameOption> additionalOptions = new();

    public string AssignedPet = null!;
    public int ChangeNameUsers;
    public int ChangeColorAndLevelUsers;
    public bool AutoDisplayResults;
    public int SuffixMode;
    public bool ColorNameMode;

    public List<GameOption> AllOptions = new();

    public MiscellaneousOptions()
    {

        AllOptions.Add(new GameOptionTitleBuilder()
            .Title(MiscOptionTranslations.MiscOptionTitle)
            .Color(_optionColor)
            .Tab(DefaultTabs.GeneralTab)
            .Build());

        GameOptionBuilder AddPets(GameOptionBuilder b)
        {
            foreach ((string? key, string? value) in ModConstants.Pets) b = b.Value(v => v.Text(key).Value(value).Build());
            return b;
        }

        AllOptions.Add(AddPets(Builder("Assigned Pet")
                .Name(MiscOptionTranslations.AssignedPetText)
                .IsHeader(true)
                .Tab(DefaultTabs.GeneralTab)
                .BindString(s => AssignedPet = s))
            .BuildAndRegister());

        AllOptions.Add(Builder("Allow /name")
            .Name(MiscOptionTranslations.AllowNameCommand)
            .Value(v => v.Value(0).Text(GeneralOptionTranslations.OffText).Color(Color.red).Build())
            .Value(v => v.Value(1).Text(GeneralOptionTranslations.FriendsText).Color(new Color(0.85f, 0.66f, 1f)).Build())
            .Value(v => v.Value(2).Text(GeneralOptionTranslations.EveryoneText).Color(Color.green).Build())
            .BindInt(b => ChangeNameUsers = b)
            .IOSettings(io => io.UnknownValueAction = ADEAnswer.UseDefault)
            .BuildAndRegister());

        AllOptions.Add(Builder("Allow /color and /level")
            .Name(MiscOptionTranslations.AllowColorAndLevelCommand)
            .Value(v => v.Value(0).Text(GeneralOptionTranslations.OffText).Color(Color.red).Build())
            .Value(v => v.Value(1).Text(GeneralOptionTranslations.FriendsText).Color(new Color(0.85f, 0.66f, 1f)).Build())
            .Value(v => v.Value(2).Text(GeneralOptionTranslations.EveryoneText).Color(Color.green).Build())
            .BindInt(b => ChangeColorAndLevelUsers = b)
            .IOSettings(io => io.UnknownValueAction = ADEAnswer.UseDefault)
            .BuildAndRegister());

        AllOptions.Add(Builder("Auto Display Results")
            .Name(MiscOptionTranslations.AutoDisplayResultsText)
            .AddOnOffValues()
            .BindBool(b => AutoDisplayResults = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Color Names")
            .Name(MiscOptionTranslations.ColorNames)
            .AddOnOffValues(false)
            .BindBool(b => ColorNameMode = b)
            .BuildAndRegister());

        additionalOptions.ForEach(o =>
        {
            o.Register();
            AllOptions.Add(o);
        });
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

        [Localized(nameof(AllowNameCommand))]
        public static string AllowNameCommand = "Allow /name";

        [Localized(nameof(AllowColorAndLevelCommand))]
        public static string AllowColorAndLevelCommand = "Allow /color and /level";

        [Localized("AutoDisplayResults")]
        public static string AutoDisplayResultsText = "Auto Display Results";

        [Localized("SuffixMode")]
        public static string SuffixModeText = "Suffix Mode";

        [Localized(nameof(ColorNames))]
        public static string ColorNames = "Color Names";
    }

}