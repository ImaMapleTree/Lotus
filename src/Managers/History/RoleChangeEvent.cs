using Lotus.Roles;
using Lotus.Extensions;
using Lotus.Options;
using VentLib.Localization.Attributes;

namespace Lotus.Managers.History;

[Localized("HistoryEvent")]
public class RoleChangeEvent: HistoryEvent
{
    private PlayerControl player;
    private CustomRole role;

    [Localized("PlayerChangedRole")]
    private static string roleChangedString;

    public RoleChangeEvent(PlayerControl player, CustomRole role)
    {
        this.player = player;
        this.role = role;
    }

    public override string CreateReport()
    {
        string timestamp = /*StaticOptions.ShowHistoryTimestamp*/true ? RelativeTimestamp() + " " : "";
        return $"{timestamp}{player.name} {roleChangedString} {role.RoleName}";
    }
}