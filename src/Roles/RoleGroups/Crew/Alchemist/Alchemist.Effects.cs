using TOHTOR.API.Meetings;
using TOHTOR.API.Vanilla.Sabotages;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Roles.RoleGroups.Crew;

public partial class Alchemist
{
    [RoleAction(RoleActionType.RoundStart)]
    private void ResetCertainEffects()
    {
        ExtraVotes = 0;
        QuickFixSabotage = false;
    }

    [RoleAction(RoleActionType.Interaction)]
    private void ProtectionEffect(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (!IsProtected) return;
        if (interaction.Intent() is INeutralIntent or IHelpfulIntent) return;
        IsProtected = false;
        handle.Cancel();
    }

    [RoleAction(RoleActionType.MyVote)]
    private void IncreasedVoting(Optional<PlayerControl> votedFor, MeetingDelegate meetingDelegate)
    {
        if (!votedFor.Exists()) return;
        for (int i = 0; i < ExtraVotes; i++) meetingDelegate.AddVote(MyPlayer, votedFor);
    }

    [RoleAction(RoleActionType.SabotagePartialFix)]
    private void SabotageQuickFix(PlayerControl fixer, ISabotage sabotage)
    {
        if (fixer.PlayerId != MyPlayer.PlayerId) return;
        sabotage.Fix(MyPlayer);
        QuickFixSabotage = false;
    }
}