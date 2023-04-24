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
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Networking.RPC;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using Object = UnityEngine.Object;

namespace TOHTOR.Roles.RoleGroups.Undead.Roles;

public class Retributionist : NeutralKillingBase
{
    private int lastVentId;
    private PlayerControl? attacker;
    private Remote<NameComponent>? remote;

    [UIComponent(UI.Cooldown)]
    private Cooldown revengeDuration;
    private bool invisibleRevenge;
    private int retributionLimit;
    private int remainingRevenges;

    [UIComponent(UI.Counter)]
    private string RevengeCounter() => retributionLimit != -1 ? RoleUtils.Counter(remainingRevenges, retributionLimit, RoleColor) : "";

    protected override void PostSetup()
    {
        remainingRevenges = retributionLimit;
        MyPlayer.NameModel().GetComponentHolder<CooldownHolder>()[0].SetPrefix("Time until Death: ").SetTextColor(RoleColor);
    }

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
        if (retributionLimit != -1 && remainingRevenges == 0) return;
        if (revengeDuration.NotReady()) return;
        if (interaction.Intent() is not IFatalIntent) return;

        remainingRevenges--;
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
        if (invisibleRevenge) DoInvisibility();
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

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Revenge Time Limit")
                .AddFloatRange(5, 60, 2.5f, 2, "s")
                .BindFloat(revengeDuration.SetDuration)
                .Build())
            .SubOption(sub => sub.Name("Invisible During Revenge")
                .AddOnOffValues()
                .BindBool(b => invisibleRevenge = b)
                .Build())
            .SubOption(sub => sub.Name("Number of Revenges")
                .Value(v => v.Text("∞").Value(-1).Build())
                .AddIntRange(1, 20, 1, 0)
                .BindInt(i => retributionLimit = i)
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.73f, 0.66f, 0.69f));
}