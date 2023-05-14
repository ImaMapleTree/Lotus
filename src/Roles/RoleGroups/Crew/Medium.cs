using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.API;
using VentLib.Localization.Attributes;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Crew;

[Localized("Roles.Medium")]
public partial class Medium: Crewmate, IModdable
{
    [Localized("MediumMessage")]
    private static string _mediumMessage = null!;


    [RoleAction(RoleActionType.AnyReportedBody)]
    private void MediumDetermineRole(PlayerControl reporter, GameData.PlayerInfo reported)
    {
        if (reporter.PlayerId != MyPlayer.PlayerId) return;
        IDeathEvent? deathEvent = Game.MatchData.GameHistory.GetEvents<IDeathEvent>().FirstOrDefault(e => e.Player().PlayerId == reported.PlayerId);
        deathEvent?.InstigatorRole().IfPresent(killerRole => Async.Schedule(() => MediumSendMessage(killerRole), 2f));
    }

    private void MediumSendMessage(CustomRole killerRole)
    {
        Utils.SendMessage($"{_mediumMessage} {killerRole}", MyPlayer.PlayerId);
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor("#A680FF");
}

