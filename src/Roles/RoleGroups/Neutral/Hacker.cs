using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.API.Vanilla.Sabotages;
using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.Options;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Victory.Conditions;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Neutral;

public class Hacker: CustomRole
{
    private List<SabotageType> sabotages = new();
    private int sabotageTotal;
    private int sabotageCount;

    [UIComponent(UI.Counter)]
    private string HackerCounter() => RoleUtils.Counter(sabotageCount, sabotageTotal, RoleColor);

    [RoleAction(RoleActionType.SabotagePartialFix)]
    private void HackerFixes(ISabotage sabotage, PlayerControl fixer)
    {
        if (fixer.PlayerId != MyPlayer.PlayerId || !sabotages.Contains(sabotage.SabotageType())) return;
        sabotage.Fix(MyPlayer);
        Game.GameHistory.AddEvent(new GenericAbilityEvent(MyPlayer, $"{ModConstants.HColor1.Colorize(MyPlayer.UnalteredName())} fixed {sabotage.SabotageType()}."));
        sabotageCount++;
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
                .AddIntRange(1, 30, 1, 7)
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Lights")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Lights))
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Reactor")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Lights))
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
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(0.21f, 0.5f, 0.07f));

}