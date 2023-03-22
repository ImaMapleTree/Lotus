using System.Collections.Generic;
using System.Linq;
using Hazel;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.NeutralKilling;
using UnityEngine;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using Object = UnityEngine.Object;

namespace TOHTOR.Roles.RoleGroups.Undead.Roles;

public class Retributionist : NeutralKillingBase
{
    private Cooldown revengeDuration = new(45);

    private int lastVentId;
    private PlayerControl? attacker;
    private Remote<NameComponent>? remote;

    [DynElement(UI.Cooldown)]
    private string customCooldown() => revengeDuration.IsReady() ? "" : RoleColor.Colorize($"Time until Death: {revengeDuration}s");

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (attacker != null && target.PlayerId != attacker.PlayerId)
        {
            revengeDuration.Finish();
            return false;
        }
        if (!base.TryKill(target)) return false;
        attacker = null;
        revengeDuration.Finish();
        return true;
    }

    [RoleAction(RoleActionType.Interaction)]
    private void InitialAttack(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (interaction.Intent() is not IFatalIntent) return;
        // TODO: Options
        switch (interaction)
        {
            case IDelayedInteraction delayedInteraction:
            case IIndirectInteraction indirectInteraction:
            case IUnblockedInteraction unblockedInteraction:
                return;
            case IManipulatedInteraction manipulatedInteraction:
                break;
            case IRangedInteraction rangedInteraction:
                break;
        }

        handle.Cancel();
        attacker = actor;
        remote = attacker.NameModel().GetComponentHolder<NameHolder>().Add(new ColoredNameComponent(attacker, new Color(1f, 0.53f, 0f), GameState.Roaming, MyPlayer));
        revengeDuration.StartThenRun(CheckRevenge);
        DoInvisibility();
    }

    private void DoInvisibility()
    {
        List<Vent> vents = Object.FindObjectsOfType<Vent>().ToList();
        if (vents.Count == 0) return;
        Vent randomVent = vents.GetRandom();
        Vector2 ventPosition = randomVent.transform.position;
        Utils.Teleport(MyPlayer.NetTransform, new Vector2(ventPosition.x, ventPosition.y + 0.3636f));
        // Important: SendOption.None is necessary to prevent kicks via anticheat. In the future if this role is kicking players this is probably why
        Async.Schedule(() => RpcV2.Immediate(MyPlayer.MyPhysics.NetId, RpcCalls.EnterVent, SendOption.None).WritePacked(randomVent.Id).SendExclusive(MyPlayer.GetClientId()), NetUtils.DeriveDelay(0.5f));
        Async.Schedule(() => RpcV2.Immediate(MyPlayer.MyPhysics.NetId, RpcCalls.BootFromVent).WritePacked(randomVent.Id).Send(MyPlayer.GetClientId()), NetUtils.DeriveDelay(1.1f));
        lastVentId = randomVent.Id;
    }

    [RoleAction(RoleActionType.RoundEnd)]
    private void CheckRevenge()
    {
        if (attacker == null) return;
        remote?.Delete();
        remote = null;
        attacker.InteractWith(MyPlayer, new UnblockedInteraction(new FatalIntent(true), attacker.GetCustomRole()));
        MyPlayer.MyPhysics.RpcBootFromVent(lastVentId);
    }


    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.73f, 0.66f, 0.69f));
}