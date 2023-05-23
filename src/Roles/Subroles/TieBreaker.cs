using System.Collections.Generic;
using System.Linq;
using Lotus.API.Player;
using Lotus.API.Vanilla.Meetings;
using Lotus.Chat;
using Lotus.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Subroles;

public class TieBreaker: Subrole
{
    public override string Identifier() => "※";

    [RoleAction(RoleActionType.VotingComplete)]
    public void CheckForTie(MeetingDelegate meetingDelegate)
    {
        if (!meetingDelegate.IsTie) return;
        List<(byte, int)> votes = meetingDelegate.CurrentVoteCount().OrderByDescending(o => o.Value).Select(kv => (kv.Key, kv.Value)).ToList();
        if (votes.Count == 0) return;
        HashSet<byte> highestVotePlayers = votes.Where(v => v.Item2 == votes[0].Item2 && v.Item1 != 255).Select(v => v.Item1).ToHashSet();
        foreach (byte votedPlayer in meetingDelegate.CurrentVotes().GetValueOrDefault(MyPlayer.PlayerId, new List<Optional<byte>>()).Filter())
        {
            // The tie has been broken
            if (!highestVotePlayers.Contains(votedPlayer)) continue;
            ChatHandler.Of(Translations.TieBreakerMessage.Formatted(RoleName), RoleColor.Colorize(RoleName)).Send();
            meetingDelegate.ExiledPlayer = Players.PlayerById(votedPlayer).Map(p => p.Data).OrElse(null!);
            break;
        }
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) => 
        base.Modify(roleModifier).RoleColor(new Color(0.51f, 0.46f, 0.72f));

    [Localized(nameof(TieBreaker))]
    private static class Translations
    {
        [Localized(nameof(TieBreakerMessage))]
        public static string TieBreakerMessage = "The tie has been broken by {0}!";
    }
}