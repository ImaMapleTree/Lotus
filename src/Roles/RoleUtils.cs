using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Vanilla.Sabotages;
using Lotus.GUI;
using Lotus.Patches.Systems;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Utilities;
using Lotus.Extensions;
using Lotus.GUI.Name;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles2.Operations;
using UnityEngine;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using Random = UnityEngine.Random;

namespace Lotus.Roles;

public static class RoleUtils
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(RoleUtils));

    public static string Arrows = "→↗↑↖←↙↓↘・";

    public static string CalculateArrow(PlayerControl source, PlayerControl target, Color? color = null)
    {
        return !target.IsAlive() ? "" : CalculateArrow(source, target.GetTruePosition(), color);
    }

    public static string CalculateArrow(PlayerControl source, Vector2 target, Color? color = null)
    {
        Vector2 sourcePosition = source.GetTruePosition();
        float distance = Vector2.Distance(sourcePosition, target);
        if (distance < ModConstants.ArrowActivationMin) return color == null ? Arrows[8].ToString() : color.Value.Colorize(Arrows[8].ToString());

        float deltaX = target.x - sourcePosition.x;
        float deltaY = target.y - sourcePosition.y;

        float angle = Mathf.Atan2(deltaY, deltaX) * Mathf.Rad2Deg;
        if (angle < 0)
            angle = 360 + angle;

        int arrow = Mathf.RoundToInt(angle / 45);
        return color == null ? Arrows[arrow < 8 ?  arrow : 0].ToString() : color.Value.Colorize(Arrows[arrow < 8 ? arrow : 0].ToString());
    }

    public static IEnumerable<PlayerControl> GetPlayersWithinDistance(PlayerControl source, float distance, bool sorted = false)
    {
        Vector2 position = source.GetTruePosition();
        return GetPlayersWithinDistance(position, distance, sorted).Where(p => p.PlayerId != source.PlayerId);
    }

    public static IEnumerable<PlayerControl> GetPlayersWithinDistance(Vector2 position, float distance, bool sorted = false)
    {
        Dictionary<byte, float> distances = sorted ? new Dictionary<byte, float>() : null!;

        IEnumerable<PlayerControl> distancePlayers = Players.GetPlayers(PlayerFilter.Alive).Where(p =>
        {
            float distanceApart = Vector2.Distance(position, p.GetTruePosition());
            if (sorted) distances[p.PlayerId] = distanceApart;
            return distanceApart <= distance;
        });

        return sorted ? distancePlayers.OrderBy(p => distances[p.PlayerId]) : distancePlayers;
    }

    public static IEnumerable<PlayerControl> GetPlayersOutsideDistance(PlayerControl source, float distance)
    {
        Vector2 sourcePosition = source.GetTruePosition();
        return Players.GetPlayers(PlayerFilter.Alive).Where(p => Vector2.Distance(sourcePosition, p.GetTruePosition()) > distance);
    }

    public static void PlayReactorsForPlayer(PlayerControl player)
    {
        if (SabotagePatch.CurrentSabotage?.SabotageType() is SabotageType.Reactor) return;
        byte reactorId = GameOptionsManager.Instance.CurrentGameOptions.MapId == 2 ? (byte)21 : (byte)3;
        RpcV3.Immediate(ShipStatus.Instance.NetId, RpcCalls.RepairSystem).Write(reactorId)
            .Write(player).Write((byte)128).Send(player.GetClientId());
    }

    public static void EndReactorsForPlayer(PlayerControl player)
    {
        if (SabotagePatch.CurrentSabotage?.SabotageType() is SabotageType.Reactor) return;
        byte reactorId = GameOptionsManager.Instance.CurrentGameOptions.MapId == 2 ? (byte)21 : (byte)3;
        RpcV3.Immediate(ShipStatus.Instance.NetId, RpcCalls.RepairSystem).Write(reactorId)
            .Write(player).Write((byte)16).Send(player.GetClientId());
        RpcV3.Immediate(ShipStatus.Instance.NetId, RpcCalls.RepairSystem).Write(reactorId)
            .Write(player).Write((byte)17).Send(player.GetClientId());
    }

    public static string Counter(object numerator, object? denominator = null, Color? color = null)
    {
        color ??= new Color(0.92f, 0.77f, 0.22f);
        return denominator == null
            ? Color.white.Colorize("(" + color.Value.Colorize($"{numerator}") + ")")
            : Color.white.Colorize("(" + color.Value.Colorize($"{numerator}/{denominator}") + ")");
    }

    public static string Cooldown(Cooldown cooldown, Color? color1 = null, Color? color2 = null)
    {
        color1 ??= new Color(0.93f, 0.57f, 0.28f);
        color2 ??= Color.white;
        return cooldown.ToString() == "0" ? "" : $"{color1.Value.Colorize("CD:")} {color2.Value.Colorize(cooldown + "s")}";
    }

    public static string ProgressBar(int current, int max, Color? color1 = null, Color? color2 = null)
    {
        color1 ??= new Color(0.92f, 0.77f, 0.22f);
        color2 ??= Color.gray;
        int diff = max - current;
        return "[" + color1.Value.Colorize("■".Repeat(current - 1) + color2.Value.Colorize("■".Repeat(diff - 1))) + "]";
    }

    public static string HealthBar(int current, int max, Color? color1 = null, Color? color2 = null)
    {
        color1 ??= new Color(0.92f, 0.77f, 0.22f);
        color2 ??= Color.gray;
        int diff = max - current;
        return TextUtils.ApplyMark("l".Repeat(current - 1), color1.Value) + TextUtils.ApplyMark("l".Repeat(diff - 1), color2.Value);
    }

    public static InteractionResult InteractWith(this PlayerControl player, PlayerControl target, Interaction interaction)
    {
        if (++Game.RecursiveCallCheck > ModConstants.RecursiveDepthLimit)
        {
            log.Warn($"Infinite Recursion detected during interaction: {interaction}", "InfiniteRecursionDetection");
            log.Trace($"Infinite Recursion Stack: {new StackTrace()}", "InfiniteRecursionDetection");
            return InteractionResult.Halt;
        }
        ActionHandle handle = ActionHandle.NoInit();
        RoleOperations.Current.TriggerForAll(LotusActionType.Interaction, target, handle,player, interaction);
        if (handle.Cancellation is ActionHandle.CancelType.None or ActionHandle.CancelType.Soft || interaction.IsPromised) interaction.Intent.Action(player, target);
        else if (handle.Cancellation is ActionHandle.CancelType.Normal) interaction.Intent.Halted(player, target);
        return handle.IsCanceled && !interaction.IsPromised ? InteractionResult.Halt : InteractionResult.Proceed;
    }

    public static void ShowGuardianShield(PlayerControl target) {
        PlayerControl? randomPlayer = Players.GetPlayers().FirstOrDefault(p => p.PlayerId != target.PlayerId);
        if (randomPlayer == null) return;

        RpcV3.Immediate(target.NetId, RpcCalls.ProtectPlayer).Write(target).Write(0).Send(target.GetClientId());
        Async.Schedule(() => RpcV3.Immediate(randomPlayer.NetId, RpcCalls.MurderPlayer).Write(target).Send(target.GetClientId()), NetUtils.DeriveDelay(0.1f));
    }


    public static void SwapPositions(PlayerControl player1, PlayerControl player2)
    {
        if (player1.inVent) player1.MyPhysics.ExitAllVents();
        if (player2.inVent) player2.MyPhysics.ExitAllVents();

        player1.MyPhysics.ResetMoveState();
        player2.MyPhysics.ResetMoveState();

        Vector2 player1Position = player1.GetTruePosition();
        Vector2 player2Position = player2.GetTruePosition();

        if (player1.IsAlive())
            Utils.Teleport(player1.NetTransform, new Vector2(player2Position.x, player2Position.y + 0.3636f));
        if (player2.IsAlive())
            Utils.Teleport(player2.NetTransform, new Vector2(player1Position.x, player1Position.y + 0.3636f));

        player1.moveable = true;
        player2.moveable = true;
        player1.Collider.enabled = true;
        player2.Collider.enabled = true;
        player1.NetTransform.enabled = true;
        player2.NetTransform.enabled = true;
    }

    public static Action<bool> BindOnOffListSetting<T>(List<T> list, T obj)
    {
        return b =>
        {
            if (!b) list.Remove(obj);
            else if (!list.Contains(obj)) list.Add(obj);
        };
    }

    public static bool RandomSpawn(CustomRole role)
    {
        return Random.RandomRange(0, 100) < role.Chance;
    }
}