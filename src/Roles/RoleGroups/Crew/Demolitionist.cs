using TOHTOR.API;
using TOHTOR.Managers.History.Events;
using TOHTOR.Options;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Crew;

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

        var interaction = new DelayedInteraction(new FatalIntent(true, () => new BombedEvent(killer, MyPlayer)), demoTime, this);
        bool dead = MyPlayer.InteractWith(killer, interaction) is InteractionResult.Proceed;
        Game.GameHistory.AddEvent(new DemolitionistBombEvent(MyPlayer, killer, dead));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.CrewmateTab)
            .SubOption(sub => sub
                .Name("Demo Time")
                .BindFloat(v => demoTime = v)
                .AddFloatRange(0.5f, 10f, 0.5f, 2)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor("#5e2801");

    private class DemolitionistBombEvent : KillEvent, IRoleEvent
    {
        public DemolitionistBombEvent(PlayerControl killer, PlayerControl victim, bool successful = true) : base(killer, victim, successful)
        {
        }
    }
}