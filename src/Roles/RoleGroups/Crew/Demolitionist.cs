using Lotus.API.Odyssey;
using Lotus.Managers.History.Events;
using Lotus.Options;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.API;
using Lotus.Extensions;
using Lotus.Roles.Subroles;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Crew;

public class Demolitionist : Crewmate
{
    private float demoTime;
    
    [RoleAction(RoleActionType.MyDeath)]
    private void DemoDeath(PlayerControl killer)
    {
        RoleUtils.PlayReactorsForPlayer(killer);
        Async.Schedule(() => DelayedDeath(killer), demoTime);
    }

    private void DelayedDeath(PlayerControl killer)
    {
        RoleUtils.EndReactorsForPlayer(killer);
        if (Game.State is not GameState.Roaming) return;
        if (killer.Data.IsDead || killer.inVent) return;

        BombedEvent bombedEvent = new(killer, MyPlayer);
        var interaction = new DelayedInteraction(new FatalIntent(true, () => bombedEvent), demoTime, this);
        MyPlayer.InteractWith(killer, interaction);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.CrewmateTab)
            .SubOption(sub => sub
                .KeyName("Demo Time", Translations.Options.DemoTime)
                .BindFloat(v => demoTime = v)
                .AddFloatRange(0.5f, 30f, 0.25f, 6, "s")
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor("#5e2801");

    [Localized(nameof(Demolitionist))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            public static string DemoTime = "Demo Time";
        }
    }
}