using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Meetings;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Impostors;

public class PickPocket : Impostor
{
    private int maximumVotes;
    private int currentVotes;
    private bool resetVotesAfterMeeting;

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
        for (int i = 0; i < currentVotes; i++) meetingDelegate.CastVote(MyPlayer, target);
    }

    [RoleAction(RoleActionType.RoundStart)]
    private void ResetVoteCounter()
    {
        if (resetVotesAfterMeeting) currentVotes = 0;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.KeyName("Maximum Additional Votes", Translations.Options.MaximumAdditionalVotes)
                .AddIntRange(1, 14, 1, 4)
                .BindInt(i => maximumVotes = i)
                .Build())
            .SubOption(sub => sub.KeyName("Reset Votes After Meetings", Translations.Options.ResetVotesAfterMeeting)
                .AddOnOffValues()
                .BindBool(b => resetVotesAfterMeeting = b)
                .Build());

    [Localized(nameof(PickPocket))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(MaximumAdditionalVotes))]
            public static string MaximumAdditionalVotes = "Maximum Additional Votes";

            [Localized(nameof(ResetVotesAfterMeeting))]
            public static string ResetVotesAfterMeeting = "Reset Votes After Meetings";
        }
    }
}