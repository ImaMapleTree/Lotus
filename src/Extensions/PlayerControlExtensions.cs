#nullable enable
using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Managers;
using Lotus.Roles;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.Subroles;
using Lotus.Patches.Actions;
using UnityEngine;
using VentLib.Utilities.Extensions;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using GameStates = Lotus.API.GameStates;

namespace Lotus.Extensions;

public static class PlayerControlExtensions
{
    public static UniquePlayerId UniquePlayerId(this PlayerControl player) => API.Player.UniquePlayerId.From(player);

    public static void Trigger(this PlayerControl player, RoleActionType action, ref ActionHandle handle, params object[] parameters)
    {
        if (player == null) return;
        CustomRole role = player.GetCustomRole();
        List<CustomRole> subroles = player.GetSubroles();
        role.Trigger(action, ref handle, parameters);
        if (handle is { IsCanceled: true }) return;
        foreach (CustomRole subrole in subroles)
        {
            subrole.Trigger(action, ref handle, parameters);
            if (handle is { IsCanceled: true }) return;
        }
    }

    public static CustomRole GetCustomRole(this PlayerControl player)
    {
        if (player == null)
        {
            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            string callerMethodName = callerMethod.Name;
            string? callerClassName = callerMethod.DeclaringType.FullName;
            VentLogger.Warn(callerClassName + "." + callerMethodName + " Invalid Custom Role", "GetCustomRole");
            return CustomRoleManager.Static.Crewmate;
        }

        CustomRole? role = Game.MatchData.Roles.MainRoles.GetValueOrDefault(player.PlayerId);
        return role ?? (player.Data.Role == null ? CustomRoleManager.Default
            : player.Data.Role.Role switch
            {
                RoleTypes.Crewmate => CustomRoleManager.Static.Crewmate,
                RoleTypes.Engineer => CustomRoleManager.Static.Mechanic,
                RoleTypes.Scientist => CustomRoleManager.Static.Physicist,
                /*RoleTypes.GuardianAngel => CustomRoleManager.Static.GuardianAngel,*/
                RoleTypes.Impostor => CustomRoleManager.Static.Impostor,
                RoleTypes.Shapeshifter => CustomRoleManager.Static.Morphling,
                _ => CustomRoleManager.Default,
            });
    }

    public static CustomRole? GetSubrole(this PlayerControl player)
    {
        List<CustomRole>? role = Game.MatchData.Roles.SubRoles.GetValueOrDefault(player.PlayerId);
        if (role == null || role.Count == 0) return null;
        return role[0] as Subrole;
    }

    public static T GetSubrole<T>(this PlayerControl player) where T: CustomRole
    {
        return (T?)player.GetSubrole()!;
    }

    public static List<CustomRole> GetSubroles(this PlayerControl player)
    {
        return Game.MatchData.Roles.SubRoles.GetValueOrDefault(player.PlayerId, new List<CustomRole>());
    }

    public static void RpcSetRoleDesync(this PlayerControl player, RoleTypes role, int clientId)
    {
        if (player == null) return;
        if (AmongUsClient.Instance.ClientId == clientId)
        {
            player.SetRole(role);
            return;
        }

        RpcV3.Immediate(player.NetId, RpcCalls.SetRole).Write((ushort)role).Send(clientId);
    }

    public static void RpcMark(this PlayerControl killer, PlayerControl? target = null, int colorId = 0)
    {
        if (target == null) target = killer;

        // Host
        if (killer.AmOwner)
        {
            killer.ProtectPlayer(target, colorId);
            killer.MurderPlayer(target);
        }

        // Other Clients
        if (killer.PlayerId == 0) return;

        RpcV3.Mass()
            .Start(killer.NetId, RpcCalls.ProtectPlayer).Write(target).Write(colorId).End()
            .Start(killer.NetId, RpcCalls.MurderPlayer).Write(target).End()
            .Send(killer.GetClientId());
    }

    public static void SetKillCooldown(this PlayerControl player, float time)
    {
        if (player.AmOwner) player.SetKillTimer(time);
        else
        {
            player.GetCustomRole().SyncOptions(new List<GameOptionOverride> { new(Override.KillCooldown, time * 2)} );
            player.RpcMark();
            player.GetCustomRole().SyncOptions();
        }
    }


    public static void RpcSpecificMurderPlayer(this PlayerControl killer, PlayerControl? target = null)
    {
        if (target == null) target = killer;
        if (killer.AmOwner)
            killer.MurderPlayer(target);
        else
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, killer.GetClientId());
            messageWriter.WriteNetObject(target);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    }


    public static void RpcResetAbilityCooldown(this PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return; //ホスト以外が実行しても何も起こさない
        VentLogger.Old($"アビリティクールダウンのリセット:{target.name}({target.PlayerId})", "RpcResetAbilityCooldown");
        if (PlayerControl.LocalPlayer == target)
        {
            //targetがホストだった場合
            PlayerControl.LocalPlayer.Data.Role.SetCooldown();
        }
        else
        {
            //targetがホスト以外だった場合
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.ProtectPlayer, SendOption.None, target.GetClientId());
            writer.WriteNetObject(target);
            writer.Write(0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
    public static void RpcDesyncRepairSystem(this PlayerControl target, SystemTypes systemType, int amount)
    {
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, target.GetClientId());
        messageWriter.Write((byte)systemType);
        messageWriter.WriteNetObject(target);
        messageWriter.Write((byte)amount);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
    }

    public static T GetCustomRole<T>(this PlayerControl player) where T : CustomRole
    {
        return (player.GetCustomRole() as T) ?? player.GetSubrole<T>();
    }

    public static string? GetAllRoleName(this PlayerControl player)
    {
        if (!player) return null;
        var text = player.GetCustomRole().RoleName;
        List<CustomRole> subroles = player.GetSubroles();
        if (subroles.Count == 0) return text;

        text += subroles.StrJoin().Replace("[", " (").Replace("]", ")");
        return text;
    }

    public static string GetNameWithRole(this PlayerControl? player)
    {
        if (player == null) return "";
        return $"{player.name}" + (GameStates.IsInGame ? $"({player.GetAllRoleName()})" : "");
    }

    public static Color GetRoleColor(this PlayerControl player)
    {
        return player.GetCustomRole().RoleColor;
    }

    public static void ResetPlayerCam(this PlayerControl pc, float delay = 0f, PlayerControl? target = null)
    {
        if (pc == null || !AmongUsClient.Instance.AmHost || pc.AmOwner) return;
        if (ReferenceEquals(target, null)) target = pc;

        var systemtypes = SystemTypes.Reactor;
        if (ProjectLotus.NormalOptions.MapId == 2) systemtypes = SystemTypes.Laboratory;

        Async.Schedule(() => pc.RpcDesyncRepairSystem(systemtypes, 128), 0f + delay);
        Async.Schedule(() => pc.RpcSpecificMurderPlayer(target), 0.2f + delay);

        Async.Schedule(() => {
            pc.RpcDesyncRepairSystem(systemtypes, 16);
            if (ProjectLotus.NormalOptions.MapId == 4) //Airship用
                pc.RpcDesyncRepairSystem(systemtypes, 17);
        }, 0.4f + delay);
    }

    public static void RpcExileV2(this PlayerControl player)
    {
        VentLogger.Trace($"Exiled (V2): {player.GetNameWithRole()}");
        player.Exiled();
        RpcV3.Immediate(player.NetId, RpcCalls.Exiled, SendOption.None).Send();
    }

    public static void NoCheckStartMeeting(this PlayerControl reporter, GameData.PlayerInfo target)
    { /*サボタージュ中でも関係なしに会議を起こせるメソッド
            targetがnullの場合はボタンとなる*/
        MeetingRoomManager.Instance.AssignSelf(reporter, target);
        DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(reporter);
        reporter.RpcStartMeeting(target);
    }

    ///<summary>
    ///プレイヤーのRoleBehaviourのGetPlayersInAbilityRangeSortedを実行し、戻り値を返します。
    ///</summary>
    ///<param name="ignoreColliders">trueにすると、壁の向こう側のプレイヤーが含まれるようになります。守護天使用</param>
    ///<returns>GetPlayersInAbilityRangeSortedの戻り値</returns>
    public static List<PlayerControl> GetPlayersInAbilityRangeSorted(this PlayerControl player, bool ignoreColliders = false) => GetPlayersInAbilityRangeSorted(player, pc => true, ignoreColliders);
    ///<summary>
    ///プレイヤーのRoleBehaviourのGetPlayersInAbilityRangeSortedを実行し、predicateの条件に合わないものを除外して返します。
    ///</summary>
    ///<param name="predicate">リストに入れるプレイヤーの条件 このpredicateに入れてfalseを返すプレイヤーは除外されます。</param>
    ///<param name="ignoreColliders">trueにすると、壁の向こう側のプレイヤーが含まれるようになります。守護天使用</param>
    ///<returns>GetPlayersInAbilityRangeSortedの戻り値から条件に合わないプレイヤーを除外したもの。</returns>
    public static List<PlayerControl> GetPlayersInAbilityRangeSorted(this PlayerControl player, Predicate<PlayerControl> predicate, bool ignoreColliders = false)
    {
        var rangePlayersIL = RoleBehaviour.GetTempPlayerList();
        List<PlayerControl> rangePlayers = new();
        player.Data.Role.GetPlayersInAbilityRangeSorted(rangePlayersIL, ignoreColliders);
        foreach (var pc in rangePlayersIL)
        {
            if (predicate(pc)) rangePlayers.Add(pc);
        }
        return rangePlayers;
    }

    public static RoleTypes GetVanillaRole(this PlayerControl player) => player.GetTeamInfo().MyRole;

    public static VanillaRoleTracker.TeamInfo GetTeamInfo(this PlayerControl player) => Game.MatchData.VanillaRoleTracker.GetInfo(player.PlayerId);

    public static bool IsAlive(this PlayerControl target)
    {
        return target != null && !target.Data.IsDead && !target.Data.Disconnected && !MurderPatches.DeferredDeaths.Contains(target.PlayerId);
    }
}