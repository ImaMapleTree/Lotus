using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Undead;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.Managers;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Victory;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Undead.Roles;

public class Necromancer : UndeadRole
{
    private static Deathknight _deathknight = new Deathknight();

    [UIComponent(UI.Cooldown)]
    private Cooldown convertCooldown;
    private bool isFirstConvert = true;
    private bool immuneToPartialConverted;

    private Deathknight? myDeathknight;
    private CustomRole deathknightOriginal = null!;
    private bool disableWinCheck = false;

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        Game.GetWinDelegate().AddSubscriber(DenyWinConditions);
    }

    [RoleAction(RoleActionType.Attack)]
    private bool NecromancerConvert(PlayerControl? target)
    {
        if (target == null) return false;
        if (MyPlayer.InteractWith(target, DirectInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;
        MyPlayer.RpcGuardAndKill(target);
        if (isFirstConvert) return ConvertToDeathknight(target);
        ConvertToUndead(target);
        return false;
    }

    [RoleAction(RoleActionType.Interaction)]
    private void NecromancerImmunity(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (interaction.Intent() is not (IHostileIntent or IFatalIntent)) return;
        if (IsConvertedUndead(actor)) handle.Cancel();
        else if (immuneToPartialConverted && IsUnconvertedUndead(actor)) handle.Cancel();
    }

    // TODO: cooldown
    [RoleAction(RoleActionType.OnPet)]
    private void NecromancerConvertPet()
    {
        if (convertCooldown.NotReady()) return;
        convertCooldown.Start();
        NecromancerConvert(MyPlayer.GetPlayersInAbilityRangeSorted().FirstOrOptional().OrElse(null!));
    }

    [RoleAction(RoleActionType.MyDeath)]
    private void NecromancerDeath()
    {
        if (myDeathknight == null || !myDeathknight.MyPlayer.IsAlive() || !myDeathknight.CanBecomeNecromancer) return;
        PlayerControl player = myDeathknight.MyPlayer;
        player.GetSubroles().Remove(deathknightOriginal);
        myDeathknight = null;
        player.NameModel().GetComponentHolder<CooldownHolder>().Clear();
        player.NameModel().GetComponentHolder<CooldownHolder>().Add(new CooldownComponent(convertCooldown, GameState.Roaming, ViewMode.Additive, player));

        Game.AssignRole(player, this);
        Necromancer necromancer = player.GetCustomRole<Necromancer>();
        necromancer.isFirstConvert = false;
        Game.GameHistory.AddEvent(new RoleChangeEvent(player, necromancer));
        disableWinCheck = true;
    }

    private bool ConvertToDeathknight(PlayerControl target)
    {
        isFirstConvert = false;

        ConvertToUndead(target);
        InitiateUndead(target);

        deathknightOriginal = target.GetCustomRole();
        CustomRoleManager.PlayerSubroles.GetOrCompute(target.PlayerId, () => new List<CustomRole>()).Add(deathknightOriginal);
        Game.AssignRole(target, _deathknight);
        myDeathknight = target.GetCustomRole<Deathknight>();
        target.NameModel().GetComponentHolder<RoleHolder>()[^1]
            .SetViewerSupplier(() => Game.GetAllPlayers().Where(p => p.PlayerId == target.PlayerId || p.Relationship(target) is Relation.FullAllies).ToList());

        VentLogger.Fatal($"Indicator count 22: {target.NameModel().GetComponentHolder<IndicatorHolder>().Count}");
        Game.GameHistory.AddEvent(new RoleChangeEvent(target, _deathknight));
        return false;
    }

    private void DenyWinConditions(WinDelegate winDelegate)
    {
        if (disableWinCheck) return;
        List<PlayerControl> winners = winDelegate.GetWinners();
        if (winners.Any(p => p.PlayerId == MyPlayer.PlayerId)) return;
        List<PlayerControl> undeadWinners = winners.Where(p => p.GetCustomRole().Faction is TheUndead).ToList();

        if (undeadWinners.Count(IsConvertedUndead) == winners.Count) winDelegate.CancelGameWin();
        else if (undeadWinners.Count == winners.Count && MyPlayer.IsAlive()) winDelegate.CancelGameWin();
        else undeadWinners.Where(tc => IsConvertedUndead(tc) || MyPlayer.IsAlive() && IsUnconvertedUndead(tc)).ForEach(uw => winners.Remove(uw));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Convert Cooldown")
                .AddFloatRange(15f, 120f, 5f, 9, "s")
                .BindFloat(convertCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub.Name("Immune to Partially Converted")
                .AddOnOffValues()
                .BindBool(b => immuneToPartialConverted = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.61f, 0.53f, 0.67f))
            .CanVent(false)
            .OptionOverride(Override.KillCooldown, convertCooldown.Duration * 2)
            .LinkedRoles(_deathknight);
}