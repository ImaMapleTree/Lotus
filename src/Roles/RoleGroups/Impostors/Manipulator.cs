using System;
using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Impostors;

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
        if (affectAnonymousMeeting) overrides.Add(new GameOptionOverride(Override.AnonymousVoting, !OriginalOptions.AnonymousVotes()));
        overrides.Add(new GameOptionOverride(Override.DiscussionTime, Math.Max(OriginalOptions.DiscussionTime() - discussionTimeDecrease, 1f)));
        overrides.Add(new GameOptionOverride(Override.VotingTime, Math.Max(OriginalOptions.DiscussionTime() - votingTimeDecrease, 1f)));
        Game.GetAllPlayers().ForEach(p => p.GetCustomRole().SyncOptions(overrides));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Kill Cooldown")
                .AddFloatRange(2f, 120f, 2.5f, 16, "s")
                .BindFloat(f => KillCooldown = f)
                .Build())
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