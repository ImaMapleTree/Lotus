using System;
using Lotus.API.Player;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
// ReSharper disable InconsistentNaming

namespace Lotus.Roles.Internals.Trackers;

[NewOnSetup]
[Localized("Roles.Miscellaneous.PlayerSelector")]
public class MeetingPlayerSelector: IPlayerSelector, ICloneOnSetup<MeetingPlayerSelector>
{
    [Localized(nameof(SelectPlayerString), ForceOverride = true)] private static string SelectPlayerString = "You have selected: <b>{0}</b>";
    [Localized(nameof(UnselectPlayerString), ForceOverride = true)] private static string UnselectPlayerString = "You have unselected: <b>{0}</b>";
    [Localized(nameof(ConfirmationString), ForceOverride = true)] private static string ConfirmationString = "You have confirmed: <b>{0}</b>";

    private byte selectedPlayer = byte.MaxValue;
    private bool confirmed;
    private VoteResultType resultOnSkip;
    private ConfirmationType confirmationType;

    public MeetingPlayerSelector(VoteResultType resultOnSkip = VoteResultType.Skipped, ConfirmationType confirmationType = ConfirmationType.DoubleVote)
    {
        this.resultOnSkip = resultOnSkip;
        this.confirmationType = confirmationType;
    }

    public VoteResult CastVote(Optional<PlayerControl> player)
    {
        if (confirmed) return Confirm();
        if (!player.Exists())
            return resultOnSkip switch
            {
                VoteResultType.None => new VoteResult(VoteResultType.None, selectedPlayer),
                VoteResultType.Skipped => new VoteResult(VoteResultType.Skipped, selectedPlayer),
                VoteResultType.Selected => Select(selectedPlayer),
                VoteResultType.Unselected => Unselect(),
                VoteResultType.Confirmed => Confirm(),
                _ => throw new ArgumentOutOfRangeException()
            };

        PlayerControl voted = player.Get();
        byte playerId = voted.PlayerId;

        return confirmationType switch
        {
            ConfirmationType.DoubleVote => playerId == selectedPlayer ? Confirm() : Select(playerId),
            ConfirmationType.Skip => Select(playerId),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private VoteResult Select(byte playerId)
    {
        selectedPlayer = playerId;
        return new VoteResult(VoteResultType.Selected, selectedPlayer, SelectPlayerString.Formatted(SelectedPlayer()?.name));
    }

    private VoteResult Unselect()
    {
        selectedPlayer = byte.MaxValue;
        return new VoteResult(VoteResultType.Unselected, selectedPlayer, UnselectPlayerString.Formatted(SelectedPlayer()?.name));
    }

    private VoteResult Confirm()
    {
        bool alreadyConfirmed = confirmed;
        confirmed = true;
        return new VoteResult(alreadyConfirmed ? VoteResultType.None : VoteResultType.Confirmed, selectedPlayer, ConfirmationString.Formatted(SelectedPlayer()?.name));
    }


    public void Reset()
    {
        selectedPlayer = byte.MaxValue;
        confirmed = false;
    }

    public PlayerControl? SelectedPlayer() => Players.FindPlayerById(selectedPlayer);

    public MeetingPlayerSelector Clone()
    {
        return (MeetingPlayerSelector)this.MemberwiseClone();
    }
}

public enum ConfirmationType
{
    DoubleVote,
    Skip
}