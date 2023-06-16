using System;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.API.Vanilla.Meetings;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Internals;
using Lotus.RPC;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

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

    [RoleAction(RoleActionType.Unshapeshift)]
    private void CamouflagerUnshapeshift(ActionHandle handle)
    {
        if (!camouflaged) return;
        camouflaged = false;
        Game.GetAlivePlayers().Where(p => p.PlayerId != MyPlayer.PlayerId).Do(p => p.CRpcRevertShapeshift(true));
    }

    [RoleAction(RoleActionType.MeetingCalled)]
    private void HandleMeetingCall(PlayerControl reporter, Optional<GameData.PlayerInfo> reported, ActionHandle handle)
    {
        if (!camouflaged) return;
        camouflaged = false;
        Game.GetAlivePlayers().Where(p => p.PlayerId != MyPlayer.PlayerId).Do(p => p.CRpcRevertShapeshift(true));
        handle.Cancel();
        Async.Schedule(() => MeetingPrep.PrepMeeting(reporter, reported.OrElse(null!)), 0.5f);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Camouflage Cooldown")
                .Bind(v => ShapeshiftCooldown = (float)v)
                .AddFloatRange(5, 120, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Camouflage Duration")
                .Bind(v => ShapeshiftDuration = (float)v)
                .AddFloatRange(5, 60, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Can Vent")
                .Bind(v => canVent = (bool)v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).CanVent(canVent);
}