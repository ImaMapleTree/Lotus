using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace Lotus.API.Vanilla.Sabotages;

public class ElectricSabotage : ISabotage
{
    private UnityOptional<PlayerControl> caller;

    public ElectricSabotage(PlayerControl? player = null)
    {
        caller = player == null ? UnityOptional<PlayerControl>.Null() : UnityOptional<PlayerControl>.NonNull(player);
    }

    public SabotageType SabotageType() => Sabotages.SabotageType.Lights;

    public bool Fix(PlayerControl? fixer = null) {
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.SabotageFixed, ref handle, this, fixer == null ? PlayerControl.LocalPlayer : fixer);
        if (handle.IsCanceled) return false;

        if (!ShipStatus.Instance.TryGetSystem(SabotageType().ToSystemType(), out ISystemType? systemInstance)) return false;
        SwitchSystem? electrical = systemInstance!.TryCast<SwitchSystem>();
        if (electrical == null)
        {
            VentLogger.Warn($"Error Fixing Lights Sabotage. Invalid System Cast from {SabotageType()}.");
            return false;
        }

        // Requires scheduling since lights actions happen in the prefix and if this is called it'll cause an issue
        Async.Schedule(() =>
        {
            electrical.ActualSwitches = electrical.ExpectedSwitches;
            electrical.IsDirty = true;
            SabotagePatch.CurrentSabotage = null;
        }, 0.05f);

        Hooks.SabotageHooks.SabotageFixedHook.Propagate(new SabotageFixHookEvent(fixer, this));
        return true;
    }

    public Optional<PlayerControl> Caller() => caller;

    public void CallSabotage(PlayerControl sabotageCaller)
    {
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.SabotageStarted, ref handle, this, sabotageCaller);
        if (handle.IsCanceled) return;

        ShipStatus.Instance.RepairSystem(SabotageType().ToSystemType(), sabotageCaller, 128);
        caller.OrElseSet(() => sabotageCaller);
        SabotagePatch.CurrentSabotage = this;
    }

    public override string ToString() => $"ElectricSabotage(Caller={caller})";
}