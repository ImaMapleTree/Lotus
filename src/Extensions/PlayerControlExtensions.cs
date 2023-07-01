#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Logging;
using Lotus.Managers.History.Events;
using Lotus.Patches.Actions;
using Lotus.Roles;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Utilities.Extensions;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Optionals;

namespace Lotus.Extensions;

public static class PlayerControlExtensions
{
    public static UniquePlayerId UniquePlayerId(this PlayerControl player) => API.Player.UniquePlayerId.From(player);

    public static void Trigger(this PlayerControl player, LotusActionType action, ref ActionHandle handle, params object[] parameters)
    {
        if (player == null) return;
        CustomRole role = player.GetCustomRole();
        List<CustomRole> subroles = player.GetSubroles();
        if (action is LotusActionType.FixedUpdate)
        {
            role.Trigger(action, ref handle, parameters);
            if (handle is { IsCanceled: true }) return;
            foreach (CustomRole subrole in subroles)
            {
                subrole.Trigger(action, ref handle, parameters);
                if (handle is { IsCanceled: true }) return;
            }
            return;
        }

        handle.ActionType = action;
        parameters = parameters.AddToArray(handle);

        foreach ((RoleAction? roleAction, AbstractBaseRole? abstractBaseRole) in role.GetActions(action).Concat(subroles.SelectMany(sr => sr.GetActions(action))).OrderBy(a => a.Item1.Priority))
        {
            DevLogger.Log(roleAction);
            PlayerControl myPlayer = abstractBaseRole.MyPlayer;
            if (handle.Cancellation is not (ActionHandle.CancelType.None or ActionHandle.CancelType.Soft)) continue;
            if (myPlayer == null || !myPlayer.IsAlive() && !roleAction.TriggerWhenDead) continue;

            try
            {
                if (action.IsPlayerAction())
                {
                    Hooks.PlayerHooks.PlayerActionHook.Propagate(new PlayerActionHookEvent(myPlayer, roleAction, parameters));
                    Game.TriggerForAll(LotusActionType.AnyPlayerAction, ref handle, myPlayer, roleAction, parameters);
                }

                handle.ActionType = action;

                if (handle.Cancellation is not (ActionHandle.CancelType.None or ActionHandle.CancelType.Soft)) continue;

                roleAction.Execute(abstractBaseRole, parameters);
            }
            catch (Exception e)
            {
                VentLogger.Exception(e, $"Failed to execute RoleAction {action}.");
            }
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
            return ProjectLotus.RoleManager.Default;
        }

        CustomRole? role = Game.MatchData.Roles.MainRoles.GetValueOrDefault(player.PlayerId);
        return role ?? ProjectLotus.RoleManager.Default;
    }

    public static CustomRole? GetCustomRoleSafe(this PlayerControl player)
    {
        if (player == null) return null;

        CustomRole? role = Game.MatchData.Roles.MainRoles.GetValueOrDefault(player.PlayerId);
        return role ?? ProjectLotus.RoleManager.Default;
    }

    public static CustomRole? GetSubrole(this PlayerControl player)
    {
        List<CustomRole>? role = Game.MatchData.Roles.SubRoles.GetValueOrDefault(player.PlayerId);
        if (role == null || role.Count == 0) return null;
        return role[0];
    }

    public static T? GetSubrole<T>(this PlayerControl player) where T: CustomRole
    {
        return player.GetSubrole() as T;
    }

    public static List<CustomRole> GetSubroles(this PlayerControl player)
    {
        return Game.MatchData.Roles.SubRoles.GetOrCompute(player.PlayerId, () => new List<CustomRole>());
    }

    public static void SyncAll(this PlayerControl player)
    {
        if (player == null) return;
        CustomRole playerRole = player.GetCustomRole();
        IEnumerable<GameOptionOverride> overrides = playerRole.CurrentRoleOverrides().Concat(player.GetSubroles().SelectMany(sr => sr.CurrentRoleOverrides()));
        playerRole.SyncOptions(overrides, parameterOverridesAreAbsolute: true);
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
        // In case of 7.11 comment out all below
        MurderPatches.Lock(killer.PlayerId);

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
        if (player.AmOwner)
        {
            player.SyncAll();
            DevLogger.Log("Synced all cooldowns");
            player.SetKillTimer(time);
        }
        else
        {
            // IMPORTANT: This could be a possible "issue" area
            CustomRole playerRole = player.GetCustomRole();
            Remote<GameOptionOverride> remote = playerRole.AddOverride(new GameOptionOverride(Override.KillCooldown, time * 2));
            playerRole.SyncOptions();

            Async.Schedule(() => player.RpcMark(), NetUtils.DeriveDelay(0.5f));

            Async.Schedule(() =>
            {
                remote.Delete();
                playerRole.SyncOptions();
            }, time);
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

    public static void RpcDesyncRepairSystem(this PlayerControl target, SystemTypes systemType, int amount)
    {
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, target.GetClientId());
        messageWriter.Write((byte)systemType);
        messageWriter.WriteNetObject(target);
        messageWriter.Write((byte)amount);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
    }

    public static T? GetCustomRole<T>(this PlayerControl player) where T : CustomRole
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
        return $"{player.name}" + (Game.State is GameState.Roaming ? $"({player.GetAllRoleName()})" : "");
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

    public static void RpcExileV2(this PlayerControl player, bool reallyExiled, bool hookDeath = true)
    {
        VentLogger.Trace($"Exiled (V2): {player.GetNameWithRole()}");
        player.Exiled();
        RpcV3.Immediate(player.NetId, RpcCalls.Exiled, SendOption.None);

        if (!reallyExiled)
        {
            if (hookDeath) Hooks.PlayerHooks.PlayerDeathHook.Propagate(new PlayerDeathHookEvent(player, new ExiledEvent(player, new List<PlayerControl>(), new List<PlayerControl>())));
            return;
        }

        ActionHandle uselessHandle = ActionHandle.NoInit();
        player.Trigger(LotusActionType.SelfExiled, ref uselessHandle);
        Game.TriggerForAll(LotusActionType.AnyExiled, ref uselessHandle, player);

        Hooks.PlayerHooks.PlayerExiledHook.Propagate(new PlayerHookEvent(player));
        Hooks.PlayerHooks.PlayerDeathHook.Propagate(new PlayerDeathHookEvent(player, new ExiledEvent(player, new List<PlayerControl>(), new List<PlayerControl>())));
    }

    public static void RpcVaporize(this PlayerControl player, PlayerControl target, IDeathEvent? deathEvent = null)
    {
        if (player == null || target == null) return;
        VentLogger.Trace($"{player.name} vaporize => {target.name}");
        target.RpcExileV2(false, false);

        MurderPatches.Lock(player.PlayerId);

        deathEvent ??= new DeathEvent(target, player);

        ActionHandle ignored = ActionHandle.NoInit();
        Optional<FrozenPlayer> fp = Optional<FrozenPlayer>.Of(Game.MatchData.GetFrozenPlayer(player));
        target.Trigger(LotusActionType.MyDeath, ref ignored, player, fp, deathEvent);
        Game.TriggerForAll(LotusActionType.AnyDeath, ref ignored, target, player, fp, deathEvent);

        PlayerMurderHookEvent playerMurderHookEvent = new(player, target, deathEvent);
        Hooks.PlayerHooks.PlayerMurderHook.Propagate(playerMurderHookEvent);
        Hooks.PlayerHooks.PlayerDeathHook.Propagate(playerMurderHookEvent);

        Async.Schedule(() =>
        {
            if (target != null) target.SetChatName(target.name);
        }, 0.1f);
    }

    public static CustomRole GetCustomRole(this GameData.PlayerInfo? playerInfo)
    {
        if (playerInfo == null) return ProjectLotus.RoleManager.Default;
        return Game.MatchData.Roles.GetMainRole(playerInfo.PlayerId);
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
        return target != null && !target.Data.IsDead && !target.Data.Disconnected;
    }
}