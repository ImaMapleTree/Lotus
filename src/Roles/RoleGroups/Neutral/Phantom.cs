using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Options;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Victory.Conditions;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Neutral;

public class Phantom : Crewmate
{
    private bool immuneToRangedInteractions;
    private int phantomClickAmt;
    private int phantomAlertAmt;
    [NewOnSetup]
    private List<Remote<IndicatorComponent>> indicatorComponents;

    [RoleAction(RoleActionType.Interaction)]
    private void PhantomInteraction(Interaction interaction, ActionHandle handle)
    {
        if (!immuneToRangedInteractions && interaction is IRangedInteraction) return;
        if (TasksComplete < phantomClickAmt) handle.Cancel();
    }

    [RoleAction(RoleActionType.MyDeath)]
    private void ClearComponents() => indicatorComponents.ForEach(c => c.Delete());

    protected override void OnTaskComplete()
    {
        if (TotalTasks == TasksComplete) ManualWin.Activate(MyPlayer, WinReason.SoloWinner, 999);
        if (TasksComplete != phantomAlertAmt) return;

        MyPlayer.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(new LiveString("★", RoleColor), GameStates.IgnStates));

        Game.GetAlivePlayers().Where(p => p.PlayerId != MyPlayer.PlayerId).ForEach(p =>
        {
            LiveString liveString = new(() => RoleUtils.CalculateArrow(p, MyPlayer, RoleColor));
            var remote = p.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(liveString, GameState.Roaming, viewers: p));
            indicatorComponents.Add(remote);
        });
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
         base.RegisterOptions(optionStream)
             .Tab(DefaultTabs.NeutralTab)
             .SubOption(sub => sub.Name("Immune to Ranged Interactions")
                 .AddOnOffValues()
                 .BindBool(b => immuneToRangedInteractions = b)
                 .Build())
             .SubOption(opt =>
                opt.Name("Tasks until Phantom Targetable")
                .BindInt(v => phantomClickAmt = v)
                .AddIntRange(1, 10, 1)
                .Build())
            .SubOption(opt =>
                opt.Name("Tasks until Phantom Alert")
                .BindInt(v => phantomAlertAmt = v)
                .AddIntRange(1, 5, 1)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        return roleModifier.RoleColor(new Color(0.4f, 0.16f, 0.38f)).SpecialType(SpecialType.Neutral);
    }
}