using System;
using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.Managers.History.Events;
using TOHTOR.Options;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Options.Game;

namespace TOHTOR.Roles.RoleGroups.NeutralKilling;

public class Pestilence: NeutralKillingBase
{
    /// <summary>
    /// A list of roles that the pestilence is immune against, this should only be populated by external addons if they want to add an immunity to pestilence lazily
    /// </summary>
    public static List<Type> ImmuneRoles = new();
    public bool ImmuneToManipulated;
    public bool ImmuneToRangedAttacks;
    public bool ImmuneToDelayedAttacks;
    public bool ImmuneToArsonist;
    public bool UnblockableAttacks;

    /// <summary>
    /// More redundancy because of how this role is done, if you have a default setting you need to be set you can add it here via an action
    /// </summary>
    public List<Action> DefaultSetters;

    public Pestilence()
    {
        DefaultSetters = new List<Action>
        {
            () => ImmuneToManipulated = false, () => ImmuneToRangedAttacks = false,
            () => ImmuneToDelayedAttacks = false, () => ImmuneToArsonist = false
        };
    }

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (!UnblockableAttacks) return base.TryKill(target);
        MyPlayer.InteractWith(target, new UnblockedInteraction(new FatalIntent(), this));
        Game.GameHistory.AddEvent(new KillEvent(MyPlayer, target));
        return true;
    }

    [RoleAction(RoleActionType.Interaction)]
    private void PestilenceAttacked(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        Intent intent = interaction.Intent();
        if (intent is not IFatalIntent) return;

        bool canceled = false;
        switch (interaction)
        {

            case IUnblockedInteraction: return;
            case IDelayedInteraction when ImmuneToDelayedAttacks:
            case IRangedInteraction when ImmuneToRangedAttacks:
                canceled = true;
                break;
            case IIndirectInteraction indirectInteraction:
                if (indirectInteraction.Emitter() is AgiTater) canceled = true;
                if (indirectInteraction.Emitter() is Arsonist && ImmuneToArsonist) canceled = true;
                break;
            case IManipulatedInteraction when ImmuneToManipulated:
            default:
                canceled = true;
                TryKill(actor);
                break;
        }

        // TODO: add immunity list
        if (canceled) handle.Cancel();
    }

    public void SetDefaultSettings()
    {
        DefaultSetters.ForEach(setter => setter());
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Tab(DefaultTabs.HiddenTab);

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.22f, 0.22f, 0.22f));
}