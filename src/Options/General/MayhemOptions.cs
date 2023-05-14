using System;
using System.Collections.Generic;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace Lotus.Options.General;

[Localized("Options")]
public class MayhemOptions
{
    private static Color _optionColor = new(0.84f, 0.8f, 1f);
    private static List<GameOption> additionalOptions = new();

    public AuMap RandomMaps;
    public bool RandomSpawn;
    public bool AllRolesCanVent;
    public bool CamoComms;

    public bool UseRandomMap => randomMapOn && RandomMaps != 0;
    private bool randomMapOn;

    public List<GameOption> AllOptions = new();

    public MayhemOptions()
    {
        AllOptions.Add(new GameOptionTitleBuilder()
            .Title(MayhemOptionTranslations.MayhemOptionTitle)
            .Color(_optionColor)
            .Tab(DefaultTabs.GeneralTab)
            .Build());

        AllOptions.Add(Builder("Enable Random Maps")
            .Name(MayhemOptionTranslations.RandomMapModeText)
            .BindBool(b => randomMapOn = b)
            .ShowSubOptionPredicate(b => (bool)b)
            .SubOption(sub => sub
                .Name(AuMap.Skeld.ToString())
                .AddOnOffValues()
                .BindBool(FlagSetter(AuMap.Skeld))
                .Build())
            .SubOption(sub => sub
                .Name(AuMap.Mira.ToString())
                .AddOnOffValues()
                .BindBool(FlagSetter(AuMap.Mira))
                .Build())
            .SubOption(sub => sub
                .Name(AuMap.Polus.ToString())
                .AddOnOffValues()
                .BindBool(FlagSetter(AuMap.Polus))
                .Build())
            .SubOption(sub => sub
                .Name(AuMap.Airship.ToString())
                .AddOnOffValues()
                .BindBool(FlagSetter(AuMap.Airship))
                .Build())
            .IsHeader(true)
            .BuildAndRegister());

        AllOptions.Add(Builder("Random Spawn")
            .Name(MayhemOptionTranslations.RandomSpawnText)
            .BindBool(b => RandomSpawn = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Camo Comms")
            .Name(MayhemOptionTranslations.CamoCommText)
            .BindBool(b => CamoComms = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("All Roles Can Vent")
            .Name(MayhemOptionTranslations.AllRolesVentText)
            .BindBool(b => AllRolesCanVent = b)
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

    private Action<bool> FlagSetter(AuMap map)
    {
        return b =>
        {
            if (b) RandomMaps |= map;
            else RandomMaps &= ~map;
        };
    }

    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.GeneralTab).Color(_optionColor).AddOnOffValues(false);

    [Localized("Mayhem")]
    private static class MayhemOptionTranslations
    {
        [Localized("SectionTitle")]
        public static string MayhemOptionTitle = "Mayhem Options";

        [Localized("RandomMaps")]
        public static string RandomMapModeText = "Enable Random Maps";

        [Localized("RandomSpawn")]
        public static string RandomSpawnText = "Random Spawn";

        [Localized("CamoComms")]
        public static string CamoCommText = "Camo Comms";

        [Localized("AllRolesCanVent")]
        public static string AllRolesVentText = "All Roles Can Vent";
    }
}

[Flags]
public enum AuMap
{
    Skeld = 1,
    Mira = 2,
    Polus = 4,
    Airship = 8
}