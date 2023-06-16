using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.API;
using Lotus.Extensions;
using Lotus.Factions;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Warlock : Shapeshifter
{
    private bool cursedPlayersKillImmediately;
    private bool limitedCurseKillRange;

    [NewOnSetup] private List<byte> cursedPlayers = null!;
    [NewOnSetup] private FixedUpdateLock fixedUpdateLock = new(ModConstants.RoleFixedUpdateCooldown);
    public bool Shapeshifted;

    [RoleAction(RoleActionType.Unshapeshift)]
    private void WarlockUnshapeshift() => Shapeshifted = false;

    [RoleAction(RoleActionType.RoundEnd)]
    private void WarlockClearCursed() => cursedPlayers.Clear();

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (Shapeshifted) return base.TryKill(target);
        if (MyPlayer.InteractWith(target, LotusInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;

        cursedPlayers.Add(target.PlayerId);
        MyPlayer.RpcMark(target);
        return true;
    }

    [RoleAction(RoleActionType.FixedUpdate)]
    private void WarlockFixedUpdate()
    {
        if (!Shapeshifted || cursedPlayersKillImmediately || !fixedUpdateLock.AcquireLock()) return;
        List<PlayerControl> actionPlayers = cursedPlayers.Filter(p => Utils.PlayerById(p)).ToList();

        foreach (PlayerControl player in actionPlayers)
        {
            if (!player.IsAlive()) cursedPlayers.Remove(player.PlayerId);
            if (KillNearestPlayer(player, true)) cursedPlayers.Remove(player.PlayerId);
        }
    }

    [RoleAction(RoleActionType.Shapeshift)]
    private void WarlockKillCheck()
    {
        Shapeshifted = true;
        foreach (PlayerControl player in cursedPlayers.Filter(b => Utils.PlayerById(b)))
        {
            if (!player.IsAlive()) continue;
            KillNearestPlayer(player, limitedCurseKillRange);
        }
        cursedPlayers.Clear();
    }

    private bool KillNearestPlayer(PlayerControl player, bool limitToRange)
    {
        List<PlayerControl> inRangePlayers = limitToRange
            ? player.GetPlayersInAbilityRangeSorted()
            : RoleUtils.GetPlayersWithinDistance(player, 9999, true).ToList();

        if (inRangePlayers.Count == 0) return false;

        PlayerControl target = inRangePlayers.GetRandom();
        ManipulatedPlayerDeathEvent playerDeathEvent = new(target, player);
        FatalIntent fatalIntent = new(false, () => playerDeathEvent);

        bool isDead = player.InteractWith(target, new ManipulatedInteraction(fatalIntent, player.GetCustomRole(), MyPlayer)) is InteractionResult.Proceed;
        Game.MatchData.GameHistory.AddEvent(new ManipulatedPlayerKillEvent(player, target, MyPlayer, isDead));

        return isDead;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddShapeshiftOptions(base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Cursed Players Kill Immediately")
                .BindBool(b => cursedPlayersKillImmediately = b)
                .AddOnOffValues()
                .ShowSubOptionPredicate(b => (bool)b)
                .SubOption(sub2 => sub2.Name("Limited Cursed Kill Range")
                    .BindBool(b => limitedCurseKillRange = b)
                    .AddOnOffValues(false)
                    .Build())
                .Build()));

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).OptionOverride(new IndirectKillCooldown(KillCooldown, () => !Shapeshifted));
}