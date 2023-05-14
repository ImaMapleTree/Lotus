using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Sabotages;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Events;
using Lotus.Roles.Internals.Attributes;
using Lotus.Victory.Conditions;
using Lotus.API;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Hacker: CustomRole
{
    private List<SabotageType> sabotages = new();
    private int sabotageTotal;
    private int sabotageCount;
    private bool fixingDoorsGivesPoint;

    [UIComponent(UI.Counter)]
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
        if (sabotageCount >= sabotageTotal) ManualWin.Activate(MyPlayer, WinReason.RoleSpecificWin, 100);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub.Name("Hacker Sabotage Amount")
                .BindInt(i => sabotageTotal = i)
                .AddIntRange(1, 60, 1, 7)
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Lights")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Lights))
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Reactor")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Reactor))
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Oxygen")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Oxygen))
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Comms")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Communications))
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Doors")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Door))
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Helicopter")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Helicopter))
                .Build())
            .SubOption(sub => sub.Name("Fixing Doors Gives Point")
                .AddOnOffValues(false)
                .BindBool(b => fixingDoorsGivesPoint = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(0.21f, 0.5f, 0.07f));

}