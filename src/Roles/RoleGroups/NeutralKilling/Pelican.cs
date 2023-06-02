using System.Collections.Generic;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.API.Stats;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Extensions;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Pelican: NeutralKillingBase
{
    private static IAccumulativeStatistic<int> _gulpedPlayerStat = Statistic<int>.CreateAccumulative("Roles.Pelican.PlayersGulped", () => Translations.GulpedStatistic);
    private static HashSet<string> _boundHooks = new();

    private bool allowPelicanEscape;

    [NewOnSetup]
    private HashSet<byte> gulpedPlayers;

    private Vector2 lastLocation;

    protected override void PostSetup()
    {
        _boundHooks.ForEach(bh => Hooks.PlayerHooks.PlayerTeleportedHook.Unbind(bh));
        _boundHooks.Clear();
        string identifier = $"{nameof(Pelican)}!{MyPlayer.PlayerId}";
        Hooks.PlayerHooks.PlayerTeleportedHook.Bind(identifier, CheckForTeleport, true);
        _boundHooks.Add(identifier);
    }

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        InteractionResult result = MyPlayer.InteractWith(target, DirectInteraction.HostileInteraction.Create(MyPlayer));
        MyPlayer.RpcMark(target);
        if (result is InteractionResult.Halt) return false;

        int randomX = Random.RandomRange(5000, 99999);
        int randomY = Random.RandomRange(5000, 99999);
        lastLocation = new Vector2(-randomX, -randomY);
        Utils.Teleport(target.NetTransform, lastLocation);
        gulpedPlayers.Add(target.PlayerId);
        _gulpedPlayerStat.Update(MyPlayer.UniquePlayerId(), i => i + 1);

        RpcV3.Immediate(MyPlayer.NetId, RpcCalls.SetScanner).Write(true).Write(++MyPlayer.scannerCount).Send(MyPlayer.GetClientId());
        Async.Schedule(() => RpcV3.Immediate(MyPlayer.NetId, RpcCalls.SetScanner).Write(false).Write(++MyPlayer.scannerCount).Send(MyPlayer.GetClientId()), 0.8f);

        return false;
    }

    [RoleAction(RoleActionType.MeetingCalled)]
    public void KillGulpedPlayers()
    {
        gulpedPlayers.Filter(Players.PlayerById).ForEach(p =>
        {
            IDeathEvent deathEvent = new CustomDeathEvent(p, MyPlayer, Translations.DeathName);
            MyPlayer.InteractWith(p, new UnblockedInteraction(new FatalIntent(true, () => deathEvent), this));
        });
        gulpedPlayers.Clear();
    }

    [RoleAction(RoleActionType.SabotageStarted)]
    public void PreventSabotageFromSwallowedPlayers(ISabotage sabotage, ActionHandle handle)
    {
        if (sabotage.Caller().Compare(s => gulpedPlayers.Contains(s.PlayerId))) handle.Cancel();
    }

    [RoleAction(RoleActionType.MyDeath)]
    public override void HandleDisconnect()
    {
        Vector2 myLocation = MyPlayer.GetTruePosition();
        gulpedPlayers.Filter(Players.PlayerById).ForEach(p => Utils.Teleport(p.NetTransform, myLocation));
    }

    public void CheckForTeleport(PlayerTeleportedHookEvent teleportedHookEvent)
    {
        PlayerControl player = teleportedHookEvent.Player;
        if (!gulpedPlayers.Contains(player.PlayerId)) return;
        if (teleportedHookEvent.NewLocation == lastLocation) return;
        if (teleportedHookEvent.NewLocation is { x: < -1000, y: < -1000 }) return;

        VentLogger.Trace($"Player: {player.name} has teleported out of the Pelican ({MyPlayer.name})", "PelicanEscape");
        gulpedPlayers.Remove(player.PlayerId);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream), "Gulp Cooldown", Translations.Options.GulpCooldown)
            .SubOption(sub => sub.KeyName("Allow Escape from Pelican", Translations.Options.AllowEscapeFromPelican)
                .BindBool(b => allowPelicanEscape = b)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.2f, 0.78f, 0.29f))
            .RoleAbilityFlags(RoleAbilityFlag.CannotSabotage | RoleAbilityFlag.CannotVent)
            .OptionOverride(new IndirectKillCooldown(KillCooldown));

    public override List<Statistic> Statistics() => new() { _gulpedPlayerStat };

    private static class Translations
    {
        [Localized(nameof(GulpedStatistic))]
        public static string GulpedStatistic = "Players Gulped";

        [Localized(nameof(DeathName))]
        public static string DeathName = "Swallowed";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(GulpCooldown))]
            public static string GulpCooldown = "Gulp Cooldown";

            [Localized(nameof(AllowEscapeFromPelican))]
            public static string AllowEscapeFromPelican = "Allow Escape from Pelican";
        }
    }

}