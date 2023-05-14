using System;
using System.Collections.Generic;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Roles.Internals;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Manipulator : Impostor
{
    private int discussionTimeDecrease;
    private int votingTimeDecrease;
    private bool affectAnonymousMeeting;
    private bool triggerAbility;
    [NewOnSetup] private HashSet<byte> killedPlayers = null!;

    [RoleAction(RoleActionType.RoundStart)]
    private void ResetKilledPlayers() => killedPlayers.Clear();

    [RoleAction(RoleActionType.AnyReportedBody)]
    public void ReportBodyAbility(GameData.PlayerInfo reported) => triggerAbility = killedPlayers.Contains(reported.PlayerId);

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        bool killed = base.TryKill(target);
        if (!killed) return false;
        killedPlayers.Add(target.PlayerId);
        return true;
    }

    [RoleAction(RoleActionType.RoundEnd)]
    public void SabotageMeeting()
    {
        List<GameOptionOverride> overrides = new();
        if (affectAnonymousMeeting) overrides.Add(new GameOptionOverride(Override.AnonymousVoting, !AUSettings.AnonymousVotes()));
        overrides.Add(new GameOptionOverride(Override.DiscussionTime, Math.Max(AUSettings.DiscussionTime() - discussionTimeDecrease, 1)));
        overrides.Add(new GameOptionOverride(Override.VotingTime, Math.Max(AUSettings.DiscussionTime() - votingTimeDecrease, 1)));
        Game.GetAllPlayers().ForEach(p => p.GetCustomRole().SyncOptions(overrides));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.Name("Discussion Time Decrease")
                .AddIntRange(0, 120, 5, 6, "s")
                .BindInt(f => discussionTimeDecrease = f)
                .Build())
            .SubOption(sub => sub.Name("Voting Time Decrease")
                .AddIntRange(0, 120, 5, 3, "s")
                .BindInt(f => votingTimeDecrease = f)
                .Build())
            .SubOption(sub => sub.Name("Affect Anonymous Voting")
                .AddOnOffValues()
                .BindBool(b => affectAnonymousMeeting = b)
                .Build());
}