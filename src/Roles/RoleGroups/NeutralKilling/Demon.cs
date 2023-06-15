using System;
using System.Collections.Generic;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Options;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Demon: NeutralKillingBase
{
    [NewOnSetup] private Dictionary<byte, Remote<NameComponent>> healthBars = null!;
    [NewOnSetup] private Dictionary<byte, int> healthInfo = null!;

    private int damagePerAttack;
    private int healthOnKill;
    private int damageTaken;

    private const int MaxHealth = 100;
    private const float MaxHealthF = MaxHealth;

    protected override void PostSetup()
    {
        Players.GetPlayers().ForEach(p => healthBars[p.PlayerId] = SetupHealthBar(p));
    }

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        InteractionResult result = MyPlayer.InteractWith(target, LotusInteraction.HostileInteraction.Create(this));
        if (result is InteractionResult.Halt) return false;
        MyPlayer.RpcMark(target);
        int remainingHealth = healthInfo.Compose(target.PlayerId, i => Mathf.Clamp(i - damagePerAttack, 0, MaxHealth));
        if (remainingHealth != 0) return false;
        healthBars[target.PlayerId].Delete();
        MyPlayer.InteractWith(target, new UnblockedInteraction(new FatalIntent(), this));
        healthInfo.Compose(MyPlayer.PlayerId, i => Mathf.Clamp(i + healthOnKill, 0, MaxHealth));
        return true;
    }

    [RoleAction(RoleActionType.Interaction, priority: Priority.Low)]
    private void InterceptAttack(Interaction interaction, ActionHandle handle)
    {
        if (interaction.Intent is not IKillingIntent || handle.IsCanceled) return;
        int remainingHealth = healthInfo.Compose(MyPlayer.PlayerId, i => Mathf.Clamp(i - damageTaken, 0, MaxHealth));

        if (remainingHealth > 0) handle.Cancel();
    }

    public override void HandleDisconnect() => CleanupHealthBar(MyPlayer);

    [RoleAction(RoleActionType.AnyExiled)]
    private void CleanupHealthBarExiled(GameData.PlayerInfo exiled)
    {
        healthBars[exiled.PlayerId].Delete();
        if (exiled.PlayerId == MyPlayer.PlayerId) healthBars.Values.ForEach(remote => remote.Delete());
    }

    [RoleAction(RoleActionType.AnyDeath)]
    private void CleanupHealthBar(PlayerControl deadPlayer)
    {
        healthBars[deadPlayer.PlayerId].Delete();
        if (deadPlayer.PlayerId == MyPlayer.PlayerId) healthBars.Values.ForEach(remote => remote.Delete());
    }

    private Remote<NameComponent> SetupHealthBar(PlayerControl player)
    {
        healthInfo[player.PlayerId] = MaxHealth;
        return player.NameModel().GCH<NameHolder>()
            .Insert(0, new NameComponent(HealthBar(player.PlayerId), new[] { GameState.Roaming }, ViewMode.Additive, MyPlayer));
    }

    private LiveString HealthBar(byte playerId)
    {
        Func<string> healthBar = () =>
        {
            int health = healthInfo[playerId];
            return RoleUtils.HealthBar(health, MaxHealth, GetHealthColor(health / MaxHealthF));
        };
        return new LiveString(() => TextUtils.ApplySize(1f, $"<cspace=-0.15em>{healthBar()}</cspace>\n"));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Attack Cooldown", Translations.Options.AttackCooldown)
                .AddFloatRange(0, 20, 0.25f, 8, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => KillCooldown = f)
                .Build())
            .SubOption(sub => sub
                .KeyName("Damage per Attack", Translations.Options.DamagePerAttack)
                .AddIntRange(5, 100, 5, 2)
                .BindInt(i => damagePerAttack = i)
                .Build())
            .SubOption(sub => sub
                .KeyName("Healing on Kill", Translations.Options.HealthHealedOnKill)
                .AddIntRange(5, 100, 5, 3)
                .BindInt(i => healthOnKill = i)
                .Build())
            .SubOption(sub => sub
                .KeyName("Damage Taken on Attack", Translations.Options.DamageTakenOnAttack)
                .AddIntRange(5, 100, 5, 6)
                .BindInt(i => damageTaken = i)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.41f, 0.74f, 0.44f))
            .RoleAbilityFlags(RoleAbilityFlag.CannotSabotage)
            .OptionOverride(new IndirectKillCooldown(KillCooldown));

    private static Color GetHealthColor(float percentage)
    {
        if (percentage >= 0.8) return new Color(0.32f, 0.91f, 0.25f);
        if (percentage >= 0.6) return new Color(0.72f, 1f, 0.18f);
        if (percentage >= 0.4) return new Color(1f, 0.93f, 0.24f);
        if (percentage >= 0.2) return new Color(1f, 0.68f, 0.25f);
        return new Color(1f, 0.36f, 0.2f);
    }

    [Localized(nameof(Demon))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(AttackCooldown))]
            public static string AttackCooldown = "Attack Cooldown";

            [Localized(nameof(DamagePerAttack))]
            public static string DamagePerAttack = "Damage per Attack";

            [Localized(nameof(HealthHealedOnKill))]
            public static string HealthHealedOnKill = "Healing on Kill";

            [Localized(nameof(DamageTakenOnAttack))]
            public static string DamageTakenOnAttack = "Damage Taken on Attack";
        }
    }
}