using System.Collections.Generic;
using Il2CppSystem;
using Lotus.API.Odyssey;
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
using Lotus.Roles.Subroles;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using Type = System.Type;

namespace Lotus.Roles.RoleGroups.Crew;

public class Demolitionist : Crewmate
{
    private Cooldown demoTime;

    [RoleAction(RoleActionType.MyDeath)]
    private void DemoDeath(PlayerControl killer)
    {
        if (MyPlayer.GetSubrole<Bait>() != null)
        {
            ExplodePlayer(killer);
            return;
        }

        string formatted = Translations.YouKilledDemoMessage.Formatted(RoleName);
        Cooldown textCooldown = demoTime.Clone();
        string Indicator() => formatted + Color.white.Colorize($"{textCooldown}s");

        Remote<TextComponent> remote = killer.NameModel().GCH<TextHolder>().Add(new TextComponent(new LiveString(Indicator, Color.red), GameState.Roaming, viewers: killer));

        RoleUtils.PlayReactorsForPlayer(killer);
        Async.Schedule(() => DelayedDeath(killer, remote), demoTime.Duration);
    }

    private void DelayedDeath(PlayerControl killer, Remote<TextComponent> textRemote)
    {
        RoleUtils.EndReactorsForPlayer(killer);
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