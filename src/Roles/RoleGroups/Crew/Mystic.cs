using Lotus.API.Vanilla.Sabotages;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Subroles;
using UnityEngine;
using VentLib;
using VentLib.Networking.RPC;
using VentLib.Networking.RPC.Attributes;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Crew;

public class Mystic : Crewmate
{
    private static ModRPC reactorFlashRpc = Vents.FindRPC((uint)RoleRPC.ReactorFlash)!;

    private float flashDuration;
    private bool sendAudioAlert;

    [RoleAction(RoleActionType.AnyDeath)]
    private void MysticAnyDeath(PlayerControl deadPlayer)
    {
        if (MyPlayer.Data.IsDead) return;
        if (deadPlayer.GetSubrole<Bait>() != null) return;


        Remote<GameOptionOverride> optionOverride = AddOverride(new GameOptionOverride(Override.CrewLightMod, 0f));
        SyncOptions();

        bool didReactorAlert = false;
        if (sendAudioAlert && SabotagePatch.CurrentSabotage?.SabotageType() is not SabotageType.Reactor)
        {
            didReactorAlert = true;
            if (!MyPlayer.IsModded()) RoleUtils.PlayReactorsForPlayer(MyPlayer);
            else if (MyPlayer.IsHost()) reactorFlashRpc.InvokeTrampoline(false);
            else reactorFlashRpc.Send(new[] { MyPlayer.GetClientId() }, false);
        }

        Async.Schedule(() => MysticRevertAlert(optionOverride, didReactorAlert), NetUtils.DeriveDelay(flashDuration));
    }

    private void MysticRevertAlert(Remote<GameOptionOverride> remote, bool didReactorAlert)
    {
        remote.Delete();
        SyncOptions();
        if (!didReactorAlert) return;
        if (!MyPlayer.IsModded()) RoleUtils.EndReactorsForPlayer(MyPlayer);
        else if (MyPlayer.IsHost()) reactorFlashRpc.InvokeTrampoline(true);
        else reactorFlashRpc.Send(new[] { MyPlayer.GetClientId() }, true);
    }

    [ModRPC(RoleRPC.ReactorFlash, invocation: MethodInvocation.ExecuteAfter)]
    private static void ReactorFlash(bool finish)
    {
        if (finish) HudManager.Instance.StopReactorFlash();
        else HudManager.Instance.StartReactorFlash();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Flash Duration")
                .Bind(v => flashDuration = (float)v)
                .AddFloatRange(0.5f, 1.5f, 0.1f, 0, GeneralOptionTranslations.SecondsSuffix)
                .IOSettings(io => io.UnknownValueAction = ADEAnswer.UseDefault)
                .Build())
            .SubOption(sub => sub
                .Name("Send Audio Alert")
                .BindBool(v => sendAudioAlert = v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.3f, 0.6f, 0.9f));
}