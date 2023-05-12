using System.Linq;
using TOHTOR.API.Odyssey;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Managers;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.Subroles;

[Localized("Roles.Subroles.Sleuth")]
public class Sleuth: Subrole
{
    [Localized("SleuthMessage")]
    private static string _sleuthMessage = "You've determined that {0} was a {1}! Great work, Detective!";

    [Localized("SleuthTitle")]
    private static string _sleuthMessageTitle = "Sleuth {0}";
    
    public override string Identifier() => "◯";

    [RoleAction(RoleActionType.SelfReportBody)]
    private void SleuthReportBody(GameData.PlayerInfo deadBody)
    {
        if (deadBody.Object != null) deadBody.Object.NameModel().GetComponentHolder<RoleHolder>().Last().AddViewer(MyPlayer);

        ulong gameId = Game.GetGameID(deadBody.PlayerId);

        CustomRole role = Game.MatchData.Roles.GetMainRole(deadBody.PlayerId);
        string title = RoleColor.Colorize($"{_sleuthMessageTitle.Formatted(MyPlayer.name)}");
        Async.Schedule(() => Utils.SendMessage(_sleuthMessage.Formatted(Game.MatchData.FrozenPlayers[gameId].Name, role), MyPlayer.PlayerId, title), 1f);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => AddRestrictToCrew(base.RegisterOptions(optionStream));

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.52f, 0.74f, 1f));
}