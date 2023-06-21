using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Options;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Crew;

public class Demolitionist : Crewmate
{
    private Cooldown demoTime;
    private byte killerId = byte.MaxValue;

    [RoleAction(RoleActionType.MyDeath)]
    private void DemoDeath(PlayerControl killer, Optional<FrozenPlayer> realKiller)
    {
        killer = realKiller.FlatMap(k => new UnityOptional<PlayerControl>(k.MyPlayer)).OrElse(killer);
        killerId = killer.PlayerId;

        string formatted = Translations.YouKilledDemoMessage.Formatted(RoleName);
        Cooldown textCooldown = demoTime.Clone();
        textCooldown.Start();
        string Indicator() => formatted + Color.white.Colorize($" {textCooldown}s");

        Remote<TextComponent> remote = killer.NameModel().GCH<TextHolder>().Add(new TextComponent(new LiveString(Indicator, Color.red), GameState.Roaming, viewers: killer));

        Async.Schedule(() => DelayedDeath(killer, remote), demoTime.Duration);
    }

    [RoleAction(RoleActionType.AnyReportedBody, triggerAfterDeath: true)]
    public void DieOnBodyReport(PlayerControl reporter, GameData.PlayerInfo body, ActionHandle handle)
    {
        if (reporter.PlayerId != killerId) return;
        if (body.PlayerId != MyPlayer.PlayerId) return;
        ExplodePlayer(reporter);
        handle.Cancel();
    }

    private void DelayedDeath(PlayerControl killer, Remote<TextComponent> textRemote)
    {
        killerId = byte.MaxValue;
        textRemote.Delete();
        if (Game.State is not GameState.Roaming) return;
        if (killer.Data.IsDead || killer.inVent) return;
        ExplodePlayer(killer);
    }

    private void ExplodePlayer(PlayerControl killer)
    {
        BombedEvent bombedEvent = new(killer, MyPlayer);
        var interaction = new DelayedInteraction(new FatalIntent(true, () => bombedEvent), demoTime.Duration, this);
        MyPlayer.InteractWith(killer, interaction);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.CrewmateTab)
            .SubOption(sub => sub
                .KeyName("Demo Time", Translations.Options.DemoTime)
                .BindFloat(demoTime.SetDuration)
                .AddFloatRange(0.5f, 30f, 0.25f, 6, GeneralOptionTranslations.SecondsSuffix)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor("#5e2801");

    [Localized(nameof(Demolitionist))]
    private static class Translations
    {
        [Localized(nameof(YouKilledDemoMessage))]
        public static string YouKilledDemoMessage = "You Killed the {0}! Vent to stay alive!";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            public static string DemoTime = "Demo Time";
        }
    }
}