using System;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Subroles;

public class Unstoppable: Subrole
{
    private bool canKillUntargetable;

    public override string Identifier() => "◇";

    [RoleAction(RoleActionType.AnyInteraction)]
    public void InterceptAnyInteraction(PlayerControl player, PlayerControl target, Interaction interaction, ActionHandle handle)
    {
        DevLogger.Log($"PLayer: {player.name} INteraction: {interaction.Intent()}");
        if (player.PlayerId != MyPlayer.PlayerId) return;
        if (interaction.Intent() is not IFatalIntent fatalIntent) return;
        handle.Cancel(ActionHandle.CancelType.Complete);

        Func<IDeathEvent>? causeOfDeath = fatalIntent.CauseOfDeath().Exists() ? () => fatalIntent.CauseOfDeath().Get() : null;
        UnstoppableIntent unstoppableIntent = new(fatalIntent.IsRanged(), causeOfDeath);

        Interaction newInteraction;
        if (canKillUntargetable) newInteraction = new UnblockedInteraction(unstoppableIntent, MyPlayer.GetCustomRole());
        else newInteraction = interaction.Modify(unstoppableIntent);
        player.InteractWith(target, newInteraction);
        VentLogger.Debug($"Unstoppable Interaction Swap: {interaction} => {newInteraction}", "UnstoppableInterception");
    }

    public override bool IsAssignableTo(PlayerControl player)
    {
        return player.GetVanillaRole().IsImpostor() && base.IsAssignableTo(player);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Can Kill Untargetable Players", Translations.Options.CanKillUntargetable)
                .AddOnOffValues(false)
                .BindBool(b => canKillUntargetable = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.55f, 0f, 0.3f, 1f));


    [Localized(nameof(Unstoppable))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(CanKillUntargetable))]
            public static string CanKillUntargetable = "Can Kill Untargetable Players";
        }
    }


    public class UnstoppableIntent : Intent
    {
        private Func<IDeathEvent>? causeOfDeath;
        private bool ranged;

        public UnstoppableIntent(bool ranged = false, Func<IDeathEvent>? causeOfDeath = null)
        {
            this.ranged = ranged;
            this.causeOfDeath = causeOfDeath;
        }

        public Optional<IDeathEvent> CauseOfDeath() => Optional<IDeathEvent>.Of(causeOfDeath?.Invoke());

        public bool IsRanged() => ranged;

        public void Action(PlayerControl actor, PlayerControl target)
        {
            Optional<IDeathEvent> deathEvent = CauseOfDeath();
            actor.GetCustomRole().SyncOptions();

            Optional<IDeathEvent> currentDeathEvent = Game.MatchData.GameHistory.GetCauseOfDeath(target.PlayerId);
            deathEvent.IfPresent(death => Game.MatchData.GameHistory.SetCauseOfDeath(target.PlayerId, death));
            KillTarget(actor, target);

            ActionHandle ignored = ActionHandle.NoInit();
            if (target.IsAlive()) Game.TriggerForAll(RoleActionType.SuccessfulAngelProtect, ref ignored, target, actor);
            else currentDeathEvent.IfPresent(de => Game.MatchData.GameHistory.SetCauseOfDeath(target.PlayerId, de));
        }

        public void KillTarget(PlayerControl actor, PlayerControl target)
        {
            ProtectedRpc.CheckMurder(!ranged ? actor : target, target);
        }

        public void Halted(PlayerControl actor, PlayerControl target)
        {
            actor.RpcMark(target);
        }
    }
}