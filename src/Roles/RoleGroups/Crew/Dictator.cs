using System;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Vanilla.Meetings;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Roles.RoleGroups.Crew;

public class Dictator: Crewmate
{
    private int totalDictates;
    private int currentDictates;

    [UIComponent(UI.Counter, ViewMode.Replace, GameState.InMeeting)]
    private string DictateCounter() => RoleUtils.Counter(currentDictates, totalDictates, RoleColor);

    protected override void PostSetup() => currentDictates = totalDictates;

    [RoleAction(RoleActionType.MyVote)]
    private void DictatorVote(Optional<PlayerControl> target, MeetingDelegate meetingDelegate)
    {
        if (!target.Exists()) return;
        meetingDelegate.EndVoting(target.Get().Data);
        Game.GameHistory.AddEvent(new DictatorVoteEvent(MyPlayer, target.Get()));
        if (--currentDictates > 0) return;

        ProtectedRpc.CheckMurder(MyPlayer, MyPlayer);
        Game.GameHistory.AddEvent(new SuicideEvent(MyPlayer));

    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Number of Dictates")
                .AddIntRange(1, 15)
                .BindInt(i => totalDictates = i)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.87f, 0.61f, 0f));

    private class DictatorVoteEvent : KillEvent, IRoleEvent
    {
        public DictatorVoteEvent(PlayerControl killer, PlayerControl victim) : base(killer, victim)
        {
        }

        public override string Message() => $"{Game.GetName(Player())} lynched {Game.GetName(Target())}.";
    }
}