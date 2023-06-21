using System.Collections.Generic;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Sabotages;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Events;
using Lotus.Roles.Internals.Attributes;
using Lotus.Victory.Conditions;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using static Lotus.Roles.RoleGroups.Crew.Repairman.RepairmanTranslations.RepairmanOptionTranslations;
using static Lotus.Roles.RoleGroups.Neutral.Hacker.HackerTranslations.HackerOptionTranslations;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Hacker: Engineer
{
    private List<SabotageType> sabotages = new();
    private int sabotageTotal;
    private int sabotageCount;
    private bool fixingDoorsGivesPoint;
    private bool hackerCanVent;

    public override bool HasTasks() => false;

    [UIComponent(UI.Counter, ViewMode.Additive, GameState.Roaming, GameState.InMeeting)]
    private string HackerCounter() => RoleUtils.Counter(sabotageCount, sabotageTotal, RoleColor);

    [RoleAction(RoleActionType.SabotagePartialFix)]
    private void HackerFixes(ISabotage sabotage, PlayerControl fixer)
    {
        if (fixer.PlayerId != MyPlayer.PlayerId || !sabotages.Contains(sabotage.SabotageType())) return;
        bool result = sabotage is DoorSabotage doorSabotage ? doorSabotage.FixRoom(MyPlayer) : sabotage.Fix(MyPlayer);
        if (result) Game.MatchData.GameHistory.AddEvent(new GenericAbilityEvent(MyPlayer, $"{ModConstants.HColor1.Colorize(MyPlayer.name)} fixed {sabotage.SabotageType()}."));
    }

    [RoleAction(RoleActionType.SabotageFixed)]
    private void HackerAcquirePoints(ISabotage sabotage, PlayerControl fixer)
    {
        if (fixer.PlayerId != MyPlayer.PlayerId) return;
        if (fixingDoorsGivesPoint || sabotage.SabotageType() is not SabotageType.Door) sabotageCount++;
        CheckHackerWin();
    }

    public void CheckHackerWin()
    {
        if (sabotageCount >= sabotageTotal) ManualWin.Activate(MyPlayer, ReasonType.RoleSpecificWin, 100);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => AddVentingOptions(sub.KeyName("Hacker Can Vent", GColor(HackerCanVent))
                .AddOnOffValues()
                .BindBool(b => hackerCanVent = b)
                .ShowSubOptionPredicate(b => (bool)b))
                .Build())
            .SubOption(sub => sub.KeyName("Points Needed to Win", HackerSabotagePointAmount)
                .BindInt(i => sabotageTotal = i)
                .AddIntRange(1, 60, 1, 7)
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
                .Build())
            .SubOption(sub => sub.KeyName("Fixing Doors Gives Point", FixingDoorsGivesPoints)
                .AddOnOffValues(false)
                .BindBool(b => fixingDoorsGivesPoint = b)
                .Build());

    private string GColor(string input) => TranslationUtil.Colorize(input, RoleColor);

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(0.21f, 0.5f, 0.07f))
            .VanillaRole(hackerCanVent ? RoleTypes.Engineer : RoleTypes.Crewmate)
            .Faction(FactionInstances.Neutral)
            .SpecialType(SpecialType.Neutral);

    [Localized(nameof(Hacker))]
    internal static class HackerTranslations
    {
        [Localized(ModConstants.Options)]
        internal static class HackerOptionTranslations
        {
            [Localized(nameof(HackerSabotagePointAmount))]
            public static string HackerSabotagePointAmount = "Points Needed to Win";

            [Localized(nameof(HackerCanVent))]
            public static string HackerCanVent = "Hacker::0 Can Vent";

            [Localized(nameof(FixingDoorsGivesPoints))]
            public static string FixingDoorsGivesPoints = "Fixing Doors Gives Points";
        }
    }

}