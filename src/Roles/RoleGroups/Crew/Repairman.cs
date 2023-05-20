using System.Collections.Generic;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Roles.Events;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using static Lotus.Roles.RoleGroups.Crew.Repairman.RepairmanTranslations.RepairmanOptionTranslations;

namespace Lotus.Roles.RoleGroups.Crew;

public class Repairman: Crewmate
{
    private bool repairmanCanVent;
    private List<SabotageType> sabotages = new();
    
    [RoleAction(RoleActionType.SabotagePartialFix)]
    private void SaboMasterFixes(ISabotage sabotage, PlayerControl fixer)
    {
        if (fixer.PlayerId != MyPlayer.PlayerId || !sabotages.Contains(sabotage.SabotageType())) return;
        bool result = sabotage is DoorSabotage doorSabotage ? doorSabotage.FixRoom(MyPlayer) : sabotage.Fix(MyPlayer);
        if (result) Game.MatchData.GameHistory.AddEvent(new GenericAbilityEvent(MyPlayer, $"{ModConstants.HColor1.Colorize(MyPlayer.name)} fixed {sabotage.SabotageType()}."));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName($"{nameof(Repairman)} Can Vent", TranslationUtil.Colorize(RepairmanCanVent, RoleColor))
                .AddOnOffValues()
                .BindBool(b => repairmanCanVent = b)
                .Build())
            .SubOption(sub => sub.KeyName("Fast Fixes Lights", FastFixLights)
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Lights))
                .Build())
            .SubOption(sub => sub.KeyName("Fast Fixes Reactor", FastFixReactor)
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Reactor))
                .Build())
            .SubOption(sub => sub.KeyName("Fast Fixes Oxygen", FastFixOxygen)
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Oxygen))
                .Build())
            .SubOption(sub => sub.KeyName("Fast Fixes Comms", FastFixComms)
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Communications))
                .Build())
            .SubOption(sub => sub.KeyName("Fast Fixes Doors", FastFixDoors)
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Door))
                .Build())
            .SubOption(sub => sub.KeyName("Fast Fixes Helicopter", FastFixHelicopter)
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Helicopter))
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier)
        .VanillaRole(repairmanCanVent ? RoleTypes.Engineer : RoleTypes.Crewmate)
        .RoleColor(Color.blue);

    [Localized(nameof(Repairman))]
    internal static class RepairmanTranslations
    {
        [Localized(nameof(RepairmanFixMessage))]
        internal static string RepairmanFixMessage = "{0} fixed {1}.";

        [Localized(ModConstants.Options)]
        internal static class RepairmanOptionTranslations
        {
            [Localized(nameof(RepairmanCanVent))]
            public static string RepairmanCanVent = "Repairman::0 Can Vent";
            
            [Localized(nameof(FastFixLights))]
            public static string FastFixLights = "Fast Fixes Lights";
            
            [Localized(nameof(FastFixReactor))]
            public static string FastFixReactor = "Fast Fixes Reactor";
            
            [Localized(nameof(FastFixOxygen))]
            public static string FastFixOxygen = "Fast Fixes Oxygen";
            
            [Localized(nameof(FastFixComms))]
            public static string FastFixComms = "Fast Fixes Comms";
            
            [Localized(nameof(FastFixDoors))]
            public static string FastFixDoors = "Fast Fixes Doors";
            
            [Localized(nameof(FastFixHelicopter))]
            public static string FastFixHelicopter = "Fast Fixes Helicopter";
        }
    }
}