using System.Collections.Generic;
using System.Linq;
using Hazel;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Stats;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.GUI.Name.Interfaces;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Networking.RPC;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Crew;

public class Herbalist: Crewmate
{
    private static IAccumulativeStatistic<int> _bloomsGrown = Statistic<int>.CreateAccumulative($"Roles.{nameof(Herbalist)}.BloomsGrown", () => Translations.BloomsGrownStatistic);
    private static IAccumulativeStatistic<int> _rolesRevealed = Statistic<int>.CreateAccumulative($"Roles.{nameof(Herbalist)}.RolesRevealed", () => Translations.RolesRevealedStatistic);
    public static readonly List<Statistic> HerbalistStatistics = new() { _bloomsGrown, _rolesRevealed };
    public override List<Statistic> Statistics() => HerbalistStatistics;

    private static Color _bloomColor = new(1f, 0.63f, 0.7f);

    private float bloomTime;
    public int bloomsBeforeReveal;
    private bool revealOnBloom;

    [NewOnSetup] private Dictionary<byte, int> bloomCounts = new();
    [NewOnSetup] private HashSet<byte> blooming = new();
    [NewOnSetup] private Dictionary<byte, List<byte>> revealedPlayers = new();

    [UIComponent(UI.Cooldown)]
    private Cooldown bloomCooldown;

    [RoleAction(RoleActionType.OnPet)]
    public void PutBloomOnPlayer()
    {
        if (bloomCooldown.NotReady()) return;
        PlayerControl? closestPlayer = MyPlayer.GetPlayersInAbilityRangeSorted().FirstOrDefault();
        if (closestPlayer == null) return;
        if (blooming.Contains(closestPlayer.PlayerId)) return;
        bloomCooldown.Start();

        if (bloomCounts.GetValueOrDefault(closestPlayer.PlayerId) >= bloomsBeforeReveal)
        {
            RevealPlayer(closestPlayer);
            return;
        }

        RpcV3.Immediate(closestPlayer.NetId, RpcCalls.SetScanner, SendOption.None).Write(true).Write(++MyPlayer.scannerCount).Send(MyPlayer.GetClientId());
        Async.Schedule(() => FinishBloom(closestPlayer.PlayerId), bloomTime);
    }

    private void RevealPlayer(PlayerControl player)
    {
        INameModel nameModel = player.NameModel();
        RoleHolder roleHolder = nameModel.GCH<RoleHolder>();
        PlayerControl? viewer = !revealedPlayers.ContainsKey(player.PlayerId) ? MyPlayer
            : RoleUtils.GetPlayersWithinDistance(player, 900, true).FirstOrDefault(p => !revealedPlayers[player.PlayerId].Contains(p.PlayerId));

        if (viewer == MyPlayer) _rolesRevealed.Update(MyPlayer.UniquePlayerId(), i => i + 1);
        if (viewer == null) return;
        roleHolder.Last().AddViewer(viewer);
        revealedPlayers.GetOrCompute(player.PlayerId, () => new List<byte>()).Add(viewer.PlayerId);
    }

    private void FinishBloom(byte playerId)
    {
        blooming.Remove(playerId);
        PlayerControl? player = Players.FindPlayerById(playerId);
        if (player == null) return;
        RpcV3.Immediate(player.NetId, RpcCalls.SetScanner, SendOption.None).Write(false).Write(++MyPlayer.scannerCount).Send(MyPlayer.GetClientId());
        _bloomsGrown.Update(MyPlayer.UniquePlayerId(), i => i + 1);
        int count = bloomCounts.Compose(playerId, i => i + 1, () =>
        {
            LiveString ls = new(() =>
            {
                int count = bloomCounts.GetValueOrDefault(playerId);
                if (count < bloomsBeforeReveal) return RoleUtils.Counter(count, bloomsBeforeReveal, RoleColor);
                return _bloomColor.Colorize("✿");
            });

            player.NameModel().GCH<CounterHolder>().Add(new CounterComponent(ls, Game.IgnStates, ViewMode.Additive, MyPlayer));
            return 1;
        });

        if (!revealOnBloom || count < bloomsBeforeReveal) return;

        player.NameModel().GCH<RoleHolder>().Last().AddViewer(MyPlayer);
        revealedPlayers.GetOrCompute(player.PlayerId, () => new List<byte>()).Add(MyPlayer.PlayerId);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Time until Bloom", Translations.Options.TimeUntilBloom)
                .BindFloat(f => bloomTime = f)
                .AddFloatRange(0, 180, 5f, 3, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub.KeyName("Blooms until Role Reveal", Translations.Options.BloomsUntilRoleReveal)
                .AddIntRange(0, 10, 1, 3)
                .BindInt(i => bloomsBeforeReveal = i)
                .Build())
            .SubOption(sub => sub.KeyName("Plant Seed Cooldown", Translations.Options.PlantBloomCooldown)
                .BindFloat(bloomCooldown.SetDuration)
                .AddFloatRange(0, 180, 2.5f, 8, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub.KeyName("Reveal on Bloom", Translations.Options.RevealOnBloom)
                .BindBool(b => revealOnBloom = b)
                .AddOnOffValues(false)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.18f, 0.84f, 0.13f));

    [Localized(nameof(Herbalist))]
    private static class Translations
    {
        [Localized(nameof(BloomsGrownStatistic))]
        public static string BloomsGrownStatistic = "Blooms Grown";

        [Localized(nameof(RolesRevealedStatistic))]
        public static string RolesRevealedStatistic = "Roles Revealed";


        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(TimeUntilBloom))]
            public static string TimeUntilBloom = "Time Until Bloom";

            [Localized(nameof(BloomsUntilRoleReveal))]
            public static string BloomsUntilRoleReveal = "Blooms Until Role Reveal";

            [Localized(nameof(PlantBloomCooldown))]
            public static string PlantBloomCooldown = "Plant Bloom Cooldown";

            [Localized(nameof(RevealOnBloom))]
            public static string RevealOnBloom = "Reveal on Bloom";
        }
    }
}