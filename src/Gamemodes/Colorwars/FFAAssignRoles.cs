using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Lotus.API;
using Lotus.Managers;
using Lotus.API.Odyssey;
using VentLib.Networking.RPC;
using VentLib.Utilities;

namespace Lotus.Gamemodes.Colorwars;

// ReSharper disable once InconsistentNaming
public static class FFAAssignRoles
{
    public static void AssignRoles(List<PlayerControl> players)
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        localPlayer.SetRole(RoleTypes.Impostor);

        foreach (PlayerControl player in players)
        {
            RpcV3.Immediate(player.NetId, (byte)RpcCalls.SetRole).Write((ushort)RoleTypes.Impostor).Send(player.GetClientId());
            RpcV3.Immediate(player.NetId, (byte)RpcCalls.SetRole).Write((ushort)RoleTypes.Crewmate).SendExcluding(player.GetClientId());
            MatchData.AssignRole(player, CustomRoleManager.Static.SerialKiller);
        }

        players.Where(p => p.PlayerId != localPlayer.PlayerId).Do(p => p.SetRole(RoleTypes.Crewmate));
    }
}