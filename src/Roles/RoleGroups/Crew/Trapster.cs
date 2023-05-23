using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using static Lotus.Roles.RoleGroups.Crew.Trapster.TrapsterTranslations.TrapsterOptionTranslations;

namespace Lotus.Roles.RoleGroups.Crew;

public class Trapster : Crewmate
{
    private float trappedDuration;
    private bool trapOnIndirectKill;

    [RoleAction(RoleActionType.Interaction)]
    private void TrapperDeath(PlayerControl actor, Interaction interaction)
    {
        if (interaction.Intent() is not IFatalIntent) return;
        if (interaction is not DirectInteraction && !trapOnIndirectKill) return;

        CustomRole actorRole = actor.GetCustomRole();
        Remote<GameOptionOverride> optionOverride = actorRole.AddOverride(new GameOptionOverride(Override.PlayerSpeedMod, 0.01f));
        Async.Schedule(() =>
        {
            optionOverride.Delete();
            actorRole.SyncOptions();
        }, trappedDuration);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Traps on Indirect Kills", TrapsOnIndirectKills)
                .BindBool(b => trapOnIndirectKill = b)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .KeyName("Trapped Duration", TrappedDuration)
                .Bind(v => trappedDuration = (float)v)
                .AddFloatRange(1, 45, 0.5f, 8, "s")
                .Build());


    
    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.35f, 0.56f, 0.82f));

    [Localized(nameof(Trapster))]
    internal static class TrapsterTranslations
    {
        [Localized(ModConstants.Options)]
        internal static class TrapsterOptionTranslations
        {
            [Localized(nameof(TrapsOnIndirectKills))]
            public static string TrapsOnIndirectKills = "Traps on Indirect Kills";

            [Localized(nameof(TrappedDuration))]
            public static string TrappedDuration = "Trapped Duration";
        }
    }
}