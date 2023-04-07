using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Impostors;

public class Puppeteer: Vanilla.Impostor
{
    private DateTime lastCheck = DateTime.Now;
    private List<PlayerControl> cursedPlayers;

    private Dictionary<byte, Remote<IndicatorComponent>> playerRemotes = null!;

    protected override void Setup(PlayerControl player) => cursedPlayers = new List<PlayerControl>();
    protected override void PostSetup() => playerRemotes = new Dictionary<byte, Remote<IndicatorComponent>>();

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (MyPlayer.InteractWith(target, SimpleInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;

        Game.GameHistory.AddEvent(new ManipulatedEvent(MyPlayer, target));
        cursedPlayers.Add(target);

        playerRemotes!.GetValueOrDefault(target.PlayerId, null)?.Delete();
        IndicatorComponent component = new(new LiveString("◆", new Color(0.36f, 0f, 0.58f)), GameStates.IgnStates, viewers: MyPlayer);
        playerRemotes[target.PlayerId] = target.NameModel().GetComponentHolder<IndicatorHolder>().Add(component);

        MyPlayer.RpcGuardAndKill(target);
        return true;
    }

    [RoleAction(RoleActionType.FixedUpdate)]
    private void PuppeteerKillCheck()
    {
        double elapsed = (DateTime.Now - lastCheck).TotalSeconds;
        if (elapsed < ModConstants.RoleFixedUpdateCooldown) return;
        lastCheck = DateTime.Now;
        foreach (PlayerControl player in new List<PlayerControl>(cursedPlayers))
        {
            if (player.Data.IsDead) {
                RemovePuppet(player);
                continue;
            }

            List<PlayerControl> inRangePlayers = player.GetPlayersInAbilityRangeSorted().Where(p => p.Relationship(MyPlayer) is not Relation.FullAllies).ToList();
            if (inRangePlayers.Count == 0) continue;
            PlayerControl target = inRangePlayers.GetRandom();
            ManipulatedPlayerDeathEvent playerDeathEvent = new(target, player);
            FatalIntent fatalIntent = new(false, () => playerDeathEvent);
            bool isDead = player.InteractWith(target, new ManipulatedInteraction(fatalIntent, player.GetCustomRole(), MyPlayer)) is InteractionResult.Proceed;
            Game.GameHistory.AddEvent(new ManipulatedPlayerKillEvent(player, target, MyPlayer, isDead));
            Game.GameHistory.AddEvent(new ManipulatedPlayerKillEvent(player, target, MyPlayer, isDead));
            RemovePuppet(player);
        }

        cursedPlayers.Where(p => p.Data.IsDead).ToArray().Do(RemovePuppet);
    }

    private void RemovePuppet(PlayerControl puppet)
    {
        playerRemotes!.GetValueOrDefault(puppet.PlayerId, null)?.Delete();
        cursedPlayers.Remove(puppet);
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(Override.KillCooldown, KillCooldown * 2);
}