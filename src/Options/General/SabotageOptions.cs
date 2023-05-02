using System;
using System.Collections.Generic;
using TOHTOR.API.Vanilla.Sabotages;
using TOHTOR.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace TOHTOR.Options.General;

[Localized("Options")]
public class SabotageOptions
{
    private static Color _optionColor = new(1f, 0.94f, 0.63f);
    private static List<GameOption> additionalOptions = new();

    public int SkeldReactorCountdown;
    public bool CustomSkeldReactorCountdown => SkeldReactorCountdown != -1;

    public int SkeldOxygenCountdown;
    public bool CustomSkeldOxygenCountdown => SkeldReactorCountdown != -1;

    public int MiraReactorCountdown;
    public bool CustomMiraReactorCountdown => SkeldReactorCountdown != -1;

    public int MiraOxygenCountdown;
    public bool CustomMiraOxygenCountdown => SkeldReactorCountdown != -1;

    public int PolusReactorCountdown;
    public bool CustomPolusReactorCountdown => SkeldReactorCountdown != -1;

    public int AirshipReactorCountdown;
    public bool CustomAirshipReactorCountdown => SkeldReactorCountdown != -1;

    public SabotageType DisabledSabotages => disableSabotages ? 0 : disabledSabotageTypes;
    private bool disableSabotages;
    private SabotageType disabledSabotageTypes;

    public List<GameOption> AllOptions = new();

    public SabotageOptions()
    {
        AllOptions.Add(new GameOptionTitleBuilder()
            .Title(SabotageOptionTranslations.SabotageOptionTitle)
            .Color(_optionColor)
            .Tab(DefaultTabs.GeneralTab)
            .Build());

        AllOptions.Add(new GameOptionBuilder()
            .IsHeader(true)
            .Name(SabotageOptionTranslations.DisableSabotagesText)
            .Key("Disable Sabotages")
            .Tab(DefaultTabs.GeneralTab)
            .Color(_optionColor)
            .AddOnOffValues(false)
            .ShowSubOptionPredicate(b => (bool)b)
            .BindBool(b => this.disableSabotages = b)
            .SubOption(sub => sub
                .Key("Disable Reactor")
                .Name(SabotageOptionTranslations.DisableReactor)
                .BindBool(FlagSetter(SabotageType.Reactor))
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub
                .Key("Disable Oxygen")
                .Name(SabotageOptionTranslations.DisableOxygen)
                .BindBool(FlagSetter(SabotageType.Oxygen))
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub
                .Key("Disable Lights")
                .Name(SabotageOptionTranslations.DisableLights)
                .BindBool(FlagSetter(SabotageType.Lights))
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub
                .Key("Disable Communications")
                .Name(SabotageOptionTranslations.DisableCommunications)
                .BindBool(FlagSetter(SabotageType.Communications))
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub
                .Key("Disable Doors")
                .Name(SabotageOptionTranslations.DisableDoors)
                .BindBool(FlagSetter(SabotageType.Door))
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub
                .Key("Disable Helicopters")
                .Name(SabotageOptionTranslations.DisableHelicopters)
                .BindBool(FlagSetter(SabotageType.Helicopter))
                .AddOnOffValues(false)
                .Build())
            .BuildAndRegister());

        AllOptions.Add(Builder("Skeld Reactor Countdown")
            .Name(AuMap.Skeld + " " + SabotageOptionTranslations.ReactorCountdown)
            .BindInt(i => SkeldReactorCountdown = i)
            .BuildAndRegister());

        AllOptions.Add(Builder("Skeld Oxygen Countdown")
            .Name(AuMap.Skeld + " " + SabotageOptionTranslations.OxygenCountdown)
            .BindInt(i => SkeldOxygenCountdown = i)
            .BuildAndRegister());

        AllOptions.Add(Builder("Mira Reactor Countdown")
            .Name(AuMap.Mira + " " + SabotageOptionTranslations.ReactorCountdown)
            .BindInt(i => MiraReactorCountdown = i)
            .BuildAndRegister());

        AllOptions.Add(Builder("Mira Oxygen Countdown")
            .Name(AuMap.Mira + " " + SabotageOptionTranslations.OxygenCountdown)
            .BindInt(i => MiraOxygenCountdown = i)
            .BuildAndRegister());

        AllOptions.Add(Builder("Polus Reactor Countdown")
            .Name(AuMap.Polus + " " + SabotageOptionTranslations.ReactorCountdown)
            .BindInt(i => PolusReactorCountdown = i)
            .BuildAndRegister());

        AllOptions.Add(Builder("Airship Reactor Countdown")
            .Name(AuMap.Airship + " " + SabotageOptionTranslations.ReactorCountdown)
            .BindInt(i => AirshipReactorCountdown = i)
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

    private Action<bool> FlagSetter(SabotageType map)
    {
        return b =>
        {
            if (b) disabledSabotageTypes |= map;
            else disabledSabotageTypes &= ~map;
        };
    }

    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key)
        .Tab(DefaultTabs.GeneralTab)
        .Color(_optionColor)
        .Value(v => v.Text(SabotageOptionTranslations.DefaultValue).Value(-1).Color(Color.cyan).Build())
        .AddIntRange(5, 120, 5, 0, "s");

    [Localized("Sabotage")]
    private static class SabotageOptionTranslations
    {
        [Localized("SectionTitle")]
        public static string SabotageOptionTitle = "Sabotage Options";

        [Localized("DefaultValue")]
        public static string DefaultValue = "Default";

        [Localized("DisableSabotages")]
        public static string DisableSabotagesText = "DisableSabotages";

        [Localized(nameof(ReactorCountdown))]
        public static string ReactorCountdown = "Reactor Countdown";

        [Localized(nameof(OxygenCountdown))]
        public static string OxygenCountdown = "Oxygen Countdown";

        [Localized(nameof(DisableReactor))]
        public static string DisableReactor = "Disable Reactor";

        [Localized(nameof(DisableOxygen))]
        public static string DisableOxygen = "Disable Oxygen";

        [Localized(nameof(DisableLights))]
        public static string DisableLights = "Disable Lights";

        [Localized(nameof(DisableCommunications))]
        public static string DisableCommunications = "Disable Communications";

        [Localized(nameof(DisableDoors))]
        public static string DisableDoors = "Disable Doors";

        [Localized(nameof(DisableHelicopters))]
        public static string DisableHelicopters = "Disable Helicopter";
    }
}