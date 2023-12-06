using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.Roles2;
using VentLib.Utilities;

namespace Lotus.Extensions;

public static class RoleRelatedExtensions
{
    public static UnifiedRoleDefinition? GetPrimaryRole(this GameData.PlayerInfo? playerInfo)
    {
        return playerInfo == null ? null : Game.MatchData.Roles.GetPrimaryRole(playerInfo.PlayerId);
    }

    public static IEnumerable<UnifiedRoleDefinition> GetAllRoleDefinitions(this PlayerControl player) => Game.CurrentGameMode.MatchData.Roles.GetRoleDefinitions(player.PlayerId);

    public static string ColoredRoleName(this UnifiedRoleDefinition roleDefinition) => roleDefinition.RoleColor.Colorize(roleDefinition.Name);
}