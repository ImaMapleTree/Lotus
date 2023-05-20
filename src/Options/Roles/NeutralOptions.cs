using Lotus.Utilities;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Options.IO;
using static Lotus.ModConstants.Palette;

namespace Lotus.Options.Roles;

[Localized(ModConstants.Options)]
public class NeutralOptions
{
    public int MinimumNeutralPassiveRoles;
    public int MaximumNeutralPassiveRoles;

    public int MinimumNeutralKillingRoles;
    public int MaximumNeutralKillingRoles;

    public NeutralTeaming NeutralTeamingMode;
    public bool KnowAlliedRoles => NeutralTeamingMode is not NeutralTeaming.Disabled && knowAlliedRoles;

    private bool knowAlliedRoles;

    public NeutralOptions()
    {
        string GColor(string input) => TranslationUtil.Colorize(input, NeutralColor, PassiveColor, KillingColor);

        Builder("Neutral Teaming Mode")
            .IsHeader(true)
            .Name(GColor(NeutralOptionTranslations.NeutralTeamMode))
            .BindInt(i => NeutralTeamingMode = (NeutralTeaming)i)
            .Value(v => v.Text(GeneralOptionTranslations.DisabledText).Value(0).Color(Color.red).Build())
            .Value(v => v.Text(NeutralOptionTranslations.SameRoleText).Value(1).Color(GeneralColor2).Build())
            .Value(v => v.Text(GColor(NeutralOptionTranslations.KillerNeutral)).Value(2).Build())
            .Value(v => v.Text(GeneralOptionTranslations.AllText).Value(3).Color(GeneralColor4).Build())
            .IOSettings(io => io.UnknownValueAction = ADEAnswer.UseDefault)
            .ShowSubOptionPredicate(v => (int)v >= 1)
            .SubOption(sub => sub.KeyName("Team Knows Each Other's Roles", NeutralOptionTranslations.AlliedKnowRoles)
                .AddOnOffValues()
                .BindBool(b => knowAlliedRoles = b)
                .Build())
            .BuildAndRegister();
        
        Builder("Minimum Neutral Passive Roles")
            .IsHeader(true)
            .Name(GColor(NeutralOptionTranslations.MinimumNeutralPassiveRoles))
            .BindInt(i => MinimumNeutralPassiveRoles = i)
            .AddIntRange(0, 15)
            .BuildAndRegister();
        
        Builder("Maximum Neutral Passive Roles")
            .Name(GColor(NeutralOptionTranslations.MaximumNeutralPassiveRoles))
            .BindInt(i => MaximumNeutralPassiveRoles = i)
            .AddIntRange(0, 15)
            .BuildAndRegister();

        Builder("Minimum Neutral Killing Roles")
            .Name(GColor(NeutralOptionTranslations.MinimumNeutralKillingRoles))
            .BindInt(i => MinimumNeutralKillingRoles = i)
            .AddIntRange(0, 15)
            .BuildAndRegister();
        
        Builder("Maximum Neutral Killing Roles")
            .Name(GColor(NeutralOptionTranslations.MaximumNeutralKillingRoles))
            .BindInt(i => MaximumNeutralKillingRoles = i)
            .AddIntRange(0, 15)
            .BuildAndRegister();
    }

    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.NeutralTab);

    [Localized("RolesNeutral")]
    private static class NeutralOptionTranslations
    {
        [Localized(nameof(MinimumNeutralPassiveRoles))]
        public static string MinimumNeutralPassiveRoles = "Minimum Neutral::0 Passive::1 Roles";

        [Localized(nameof(MaximumNeutralPassiveRoles))]
        public static string MaximumNeutralPassiveRoles = "Maximum Neutral::0 Passive::1 Roles";

        [Localized(nameof(MinimumNeutralKillingRoles))]
        public static string MinimumNeutralKillingRoles = "Minimum Neutral::0 Killing::2 Roles";

        [Localized(nameof(MaximumNeutralKillingRoles))]
        public static string MaximumNeutralKillingRoles = "Maximum Neutral::0 Killing::2 Roles";

        [Localized(nameof(NeutralTeamMode))]
        public static string NeutralTeamMode = "Neutral::0 Teaming Mode";

        [Localized(nameof(SameRoleText))]
        public static string SameRoleText = "Same Role";

        [Localized("KillingAndPassiveText")]
        public static string KillerNeutral = "Killing::2 And Passive::1";

        [Localized(nameof(AlliedKnowRoles))]
        public static string AlliedKnowRoles = "Team Knows Everyone's Role";
    }
}

public enum NeutralTeaming
{
    Disabled,
    SameRole,
    KillersNeutrals,
    All
}