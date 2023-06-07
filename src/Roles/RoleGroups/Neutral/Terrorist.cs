using Lotus.Factions;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Roles.Internals;
using Lotus.Victory.Conditions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Neutral;

// Inherits from crewmate because crewmate has task setup
public class Terrorist : Engineer
{
    private bool canWinBySuicide;
    private bool canWinByExiled;

    public override bool TasksApplyToTotal() => false;

    [RoleAction(RoleActionType.MyDeath)]
    private void OnTerroristDeath(PlayerControl killer)
    {
        if (killer.PlayerId == MyPlayer.PlayerId && !canWinBySuicide) return;
        TerroristWinCheck();
    }

    [RoleAction(RoleActionType.SelfExiled)]
    private void OnExiled()
    {
        if (canWinByExiled) TerroristWinCheck();
    }

    private void TerroristWinCheck()
    {
        if (!HasAllTasksComplete) return;
        ManualWin.Activate(MyPlayer, WinReason.TasksComplete, 900);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddTaskOverrideOptions(base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub
                .KeyName("Can Win By Suicide", TerroristTranslations.TerroristOptionTranslations.CanWinBySuicide)
                .BindBool(v => canWinBySuicide = v)
                .AddOnOffValues(false)
                .Build()))
            .SubOption(sub => sub
                .KeyName("Can Win By Being Exiled", TerroristTranslations.TerroristOptionTranslations.CanWinByExiled)
                .BindBool(v => canWinByExiled = v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.52f, 0.84f, 0.28f))
            .RoleFlags(RoleFlag.CannotWinAlone)
            .Faction(FactionInstances.Neutral)
            .SpecialType(SpecialType.Neutral);

    [Localized(nameof(Terrorist))]
    internal static class TerroristTranslations
    {
        [Localized(ModConstants.Options)]
        internal static class TerroristOptionTranslations
        {
            [Localized(nameof(CanWinBySuicide))]
            public static string CanWinBySuicide = "Can Win By Suicide";

            [Localized(nameof(CanWinByExiled))]
            public static string CanWinByExiled = "Can Win By Being Exiled";
        }
    }
}