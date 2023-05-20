using System;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.API;
using Lotus.Extensions;
using Lotus.Roles.Internals;
using Lotus.RPC;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Camouflager: Shapeshifter
{
    private bool canVent;
    private DateTime lastShapeshift;
    private DateTime lastUnshapeshift;
    private bool camouflaged;

    [RoleAction(RoleActionType.Attack)]
    public new bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(RoleActionType.Shapeshift)]
    private void CamouflagerShapeshift(PlayerControl target)
    {
        if (camouflaged) return;
        camouflaged = true;
        Game.GetAlivePlayers().Where(p => p.PlayerId != MyPlayer.PlayerId).Do(p => p.CRpcShapeshift(target, true));
    }

    [RoleAction(RoleActionType.MeetingCalled)]
    [RoleAction(RoleActionType.Unshapeshift)]
    private void CamouflagerUnshapeshift(ActionHandle handle)
    {
        if (!camouflaged) return;
        camouflaged = false;
        if (handle.ActionType is not RoleActionType.MeetingCalled)
            Game.GetAlivePlayers().Where(p => p.PlayerId != MyPlayer.PlayerId).Do(p => p.CRpcRevertShapeshift(true));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Camouflage Cooldown")
                .Bind(v => ShapeshiftCooldown = (float)v)
                .AddFloatRange(5, 120, 2.5f, 5, "s")
                .Build())
            .SubOption(sub => sub
                .Name("Camouflage Duration")
                .Bind(v => ShapeshiftDuration = (float)v)
                .AddFloatRange(5, 60, 2.5f, 5, "s")
                .Build())
            .SubOption(sub => sub
                .Name("Can Vent")
                .Bind(v => canVent = (bool)v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).CanVent(canVent);
}