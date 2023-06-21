using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Managers.History.Events;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Roles.Subroles;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Networking.RPC.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Janitor: Impostor
{
    public static HashSet<Type> JanitorBannedModifiers = new() { typeof(Oblivious), typeof(Sleuth) };
    public override HashSet<Type> BannedModifiers() => cleanOnKill ? new HashSet<Type>() : JanitorBannedModifiers;

    private bool cleanOnKill;
    private float killMultiplier;

    private float JanitorKillCooldown() => cleanOnKill ? KillCooldown * killMultiplier : KillCooldown;

    [UIComponent(UI.Cooldown)]
    private Cooldown cleanCooldown;

    [RoleAction(RoleActionType.Attack)]
    public new bool TryKill(PlayerControl target)
    {
        cleanCooldown.Start(AUSettings.KillCooldown());

        if (!cleanOnKill) return base.TryKill(target);

        MyPlayer.RpcMark(target);
        if (MyPlayer.InteractWith(target, new LotusInteraction(new FakeFatalIntent(), this)) is InteractionResult.Halt) return false;
        MyPlayer.RpcVaporize(target);
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target));
        Game.MatchData.GameHistory.AddEvent(new GenericAbilityEvent(MyPlayer, $"{Color.red.Colorize(MyPlayer.name)} cleaned {target.GetRoleColor().Colorize(target.name)}."));
        return true;
    }

    [RoleAction(RoleActionType.SelfReportBody)]
    private void JanitorCleanBody(GameData.PlayerInfo target, ActionHandle handle)
    {
        if (cleanCooldown.NotReady()) return;
        handle.Cancel();
        cleanCooldown.Start();

        byte playerId = target.Object.PlayerId;

        foreach (DeadBody deadBody in Object.FindObjectsOfType<DeadBody>())
            if (deadBody.ParentId == playerId)
                if (ModVersion.AllClientsModded()) CleanBody(playerId);
                else Game.MatchData.UnreportableBodies.Add(playerId);

        MyPlayer.RpcMark(MyPlayer);
    }

    [ModRPC(RoleRPC.RemoveBody, invocation: MethodInvocation.ExecuteAfter)]
    private static void CleanBody(byte playerId)
    {
        VentLogger.Debug("Destroying Bodies", "JanitorClean");
        Object.FindObjectsOfType<DeadBody>().ToArray().Where(db => db.ParentId == playerId).ForEach(b => Object.Destroy(b.gameObject));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Clean On Kill", Translations.Options.CleanOnKill)
                .AddOnOffValues()
                .BindBool(b => cleanOnKill = b)
                .ShowSubOptionPredicate(b => (bool)b)
                .SubOption(sub2 => sub2
                    .KeyName("Kill Cooldown Multiplier", Translations.Options.KillCooldownMultiplier)
                    .AddFloatRange(1, 3, 0.25f, 2, "x")
                    .BindFloat(f => killMultiplier = f)
                    .Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(new IndirectKillCooldown(JanitorKillCooldown, () => cleanOnKill || cleanCooldown.NotReady()));

    [Localized(nameof(Janitor))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(CleanOnKill))]
            public static string CleanOnKill = "Clean On KIll";

            [Localized(nameof(KillCooldownMultiplier))]
            public static string KillCooldownMultiplier = "Kill Cooldown Multiplier";
        }
    }


    private class FakeFatalIntent : IFatalIntent
    {
        public void Action(PlayerControl actor, PlayerControl target)
        {
        }

        public void Halted(PlayerControl actor, PlayerControl target)
        {
        }

        public Optional<IDeathEvent> CauseOfDeath() => Optional<IDeathEvent>.Null();

        public bool IsRanged() => false;

        private Dictionary<string, object?>? meta;
        public object? this[string key]
        {
            get => (meta ?? new Dictionary<string, object?>()).GetValueOrDefault(key);
            set => (meta ?? new Dictionary<string, object?>())[key] = value;
        }
    }
}