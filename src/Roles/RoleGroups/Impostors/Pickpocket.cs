
using System;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Meetings;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.API;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Impostors;

public class PickPocket : Impostor
{
    private int maximumVotes;
    private int currentVotes;

    [UIComponent(UI.Counter, GameStates = new [] { GameState.Roaming, GameState.InMeeting})]
    private string VoteCounter() => RoleUtils.Counter(currentVotes, color: new Color(1f, 0.45f, 0.25f));

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        bool killed = base.TryKill(target);
        if (!killed) return false;
        if (currentVotes < maximumVotes) currentVotes++;
        return true;
    }

    [RoleAction(RoleActionType.MyVote)]
    private void EnhancedVote(Optional<PlayerControl> target, MeetingDelegate meetingDelegate)
    {
        for (int i = 0; i < currentVotes; i++) meetingDelegate.AddVote(MyPlayer, target);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.Name("Maximum Additional Votes")
                .AddIntRange(1, 14, 1, 4)
                .BindInt(i => maximumVotes = i)
                .Build());
}