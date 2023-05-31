using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Options;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.Victory.Conditions;
using Lotus.API;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace Lotus.Roles.RoleGroups.Neutral;

[Localized("Roles.Postman")]
public class Postman: Crewmate
{
    [Localized("Announcement")]
    private static string _postmanAnnouncement = "The Postman has completed all of their deliveries. Vote them otherwise they win the game.";

    private bool hasArrowToTarget;
    // 0 = Deliver to dead bodies, 1 = reassign on meeting, 2 = reassign in game
    private int targetDiesMode;

    private DateTime lastCheck = DateTime.Now;
    private const float UpdateTimeout = 0.25f;

    private PlayerControl trackedPlayer;
    private bool completedDelivery = true;
    [NewOnSetup] private List<Remote<IndicatorComponent>> components;

    public override bool TasksApplyToTotal() => false;

    protected override void OnTaskComplete(Optional<NormalPlayerTask> _)
    {
        if (completedDelivery) AssignNewTarget();
        else MyPlayer.InteractWith(MyPlayer, new UnblockedInteraction(new FatalIntent(), this));
    }

    [RoleAction(RoleActionType.AnyDeath)]
    private void CheckPlayerDeaths(PlayerControl deadPlayer)
    {
        if (deadPlayer.PlayerId == MyPlayer.PlayerId) components.ForEach(c => c.Delete());
        if (completedDelivery) return;
        if (trackedPlayer.PlayerId != deadPlayer.PlayerId) return;
        if (targetDiesMode == 0) return;
        AssignNewTarget();
    }

    [RoleAction(RoleActionType.AnyExiled)]
    private void CheckEndMeeting(GameData.PlayerInfo exiled)
    {
        if (exiled.PlayerId == MyPlayer.PlayerId) components.ForEach(c => c.Delete());
        if (completedDelivery) return;
        if (trackedPlayer.PlayerId != exiled.PlayerId) return;
        AssignNewTarget();
    }

    [RoleAction(RoleActionType.FixedUpdate)]
    private void CheckForDelivery()
    {
        if (completedDelivery) return;
        if (DateTime.Now.Subtract(lastCheck).TotalSeconds < UpdateTimeout) return;
        lastCheck = DateTime.Now;
        if (trackedPlayer.IsAlive())
            if (RoleUtils.GetPlayersWithinDistance(MyPlayer, 0.8f).All(p => p.PlayerId != trackedPlayer.PlayerId)) return;
        else if (!trackedPlayer.IsAlive())
            if (Object.FindObjectsOfType<DeadBody>().All(b => Vector2.Distance(b.TruePosition, MyPlayer.GetTruePosition()) > 0.8)) return;


        completedDelivery = true;
        components.ForEach(c => c.Delete());
        components.Clear();
    }

    [RoleAction(RoleActionType.RoundEnd)]
    private void AnnounceGameWin()
    {
        if (!completedDelivery || TasksComplete != TotalTasks) return;
        Async.Schedule(() => ChatHandler.Of(_postmanAnnouncement).Send(), 1f); 
    }

    [RoleAction(RoleActionType.RoundStart)]
    private void CheckGameWin()
    {
        if (completedDelivery && TasksComplete == TotalTasks)
            ManualWin.Activate(MyPlayer, WinReason.RoleSpecificWin, 999);
    }

    private void AssignNewTarget()
    {
        components.ForEach(c => c.Delete());
        components.Clear();
        List<PlayerControl> candidates = Game.GetAlivePlayers().Where(p => p.PlayerId != MyPlayer.PlayerId).ToList();
        if (candidates.Count == 0) return;
        completedDelivery = false; // Important

        PlayerControl candidate = candidates.GetRandom();
        trackedPlayer = candidate;
        IndicatorComponent component = new SimpleIndicatorComponent("★", RoleColor, GameState.Roaming, viewers: MyPlayer);
        components.Add(candidate.NameModel().GetComponentHolder<IndicatorHolder>().Add(component));

        if (!hasArrowToTarget) return;
        component = new IndicatorComponent(new LiveString(() => RoleUtils.CalculateArrow(MyPlayer, candidate, RoleColor)), GameState.Roaming, viewers: MyPlayer);
        components.Add(MyPlayer.NameModel().GetComponentHolder<IndicatorHolder>().Add(component));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddTaskOverrideOptions(base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub.Name("Has Arrow To Targets")
                .AddOnOffValues()
                .BindBool(b => hasArrowToTarget = b)
                .Build())
            .SubOption(sub => sub.Name("When Target Dies")
                .Value(v => v.Text("Deliver to Body").Value(0).Build())
                .Value(v => v.Text("Reassign in Meeting").Value(1).Build())
                .Value(v => v.Text("Reassign in Game").Value(2).Build())
                .BindInt(i => targetDiesMode = i)
                .Build()));

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier)
        .RoleColor(new Color(0.6f, 0.6f, 0.6f))
        .SpecialType(SpecialType.Neutral)
        .Faction(FactionInstances.Solo);
}