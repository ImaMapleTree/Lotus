extern alias JBAnnotations;
using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Meetings;
using Lotus.Factions;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Options;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Victory.Conditions;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Neutral;

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