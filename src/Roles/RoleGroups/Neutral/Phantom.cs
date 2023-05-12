extern alias JBAnnotations;
using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Vanilla.Meetings;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Options;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Victory.Conditions;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Roles.RoleGroups.Neutral;

public class Phantom : Crewmate, IPhantomRole
{
    private bool immuneToRangedInteractions;
    private int phantomClickAmt;
    private int phantomAlertAmt;
    private bool isRevealed;
    
    [NewOnSetup]
    private List<Remote<IndicatorComponent>> indicatorComponents = null!;
    
    [RoleAction(RoleActionType.Interaction)]
    private void PhantomInteraction(Interaction interaction, ActionHandle handle)
    {
        if (!immuneToRangedInteractions && interaction is IRangedInteraction) return;
        if (TasksComplete < phantomClickAmt) handle.Cancel();
    }

    [RoleAction(RoleActionType.MyDeath)]
    private void ClearComponents() => indicatorComponents.ForEach(c => c.Delete());

    [RoleAction(RoleActionType.AnyVote, priority: Priority.Last)]
    private void ClearVotesAgainstPhantom(Optional<PlayerControl> player, ActionHandle handle)
    {
        if (!isRevealed) return;
        if (!player.Exists() || player.Get().PlayerId != MyPlayer.PlayerId) return;
        handle.Cancel();
    }
    
    [RoleAction(RoleActionType.VotingComplete, priority: Priority.Last)]
    private void ClearVotedOut(MeetingDelegate meetingDelegate)
    {
        if (meetingDelegate.ExiledPlayer == null) return;
        if (meetingDelegate.ExiledPlayer.PlayerId != MyPlayer.PlayerId) return;
        meetingDelegate.ExiledPlayer = null;
        PhantomReveal();
    }

    public override bool TasksApplyToTotal() => false;
    
    public bool IsCountedAsPlayer() => false;

    protected override void OnTaskComplete()
    {
        if (!MyPlayer.IsAlive()) return;
        if (TotalTasks == TasksComplete) ManualWin.Activate(MyPlayer, WinReason.SoloWinner, 999);
        if (TasksComplete != phantomAlertAmt) return;
        PhantomReveal();
    }

    private void PhantomReveal()
    {
        MyPlayer.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(new LiveString("★", RoleColor), GameStates.IgnStates));

        Game.GetAlivePlayers().Where(p => p.PlayerId != MyPlayer.PlayerId).ForEach(p =>
        {
            LiveString liveString = new(() => RoleUtils.CalculateArrow(p, MyPlayer, RoleColor));
            var remote = p.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(liveString, GameState.Roaming, viewers: p));
            indicatorComponents.Add(remote);
        });
        isRevealed = true;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
         AddTaskOverrideOptions(base.RegisterOptions(optionStream)
             .Tab(DefaultTabs.NeutralTab)
             .SubOption(sub => sub.Name("Immune to Ranged Interactions")
                 .AddOnOffValues()
                 .BindBool(b => immuneToRangedInteractions = b)
                 .Build())
             .SubOption(opt =>
                opt.Name("Tasks until Phantom Targetable")
                .BindInt(v => phantomClickAmt = v)
                .AddIntRange(1, 40, 1)
                .Build())
            .SubOption(opt =>
                opt.Name("Tasks until Phantom Alert")
                .BindInt(v => phantomAlertAmt = v)
                .AddIntRange(1, 40, 1)
                .Build()));

    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier
        .RoleColor(new Color(0.4f, 0.16f, 0.38f))
        .SpecialType(SpecialType.Neutral)
        .Faction(FactionInstances.Solo)
        .RoleFlags(RoleFlag.CannotWinAlone);
}