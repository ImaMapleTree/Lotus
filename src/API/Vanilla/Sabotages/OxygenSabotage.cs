using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using VentLib.Logging;
using VentLib.Utilities.Optionals;

namespace Lotus.API.Vanilla.Sabotages;

public class OxygenSabotage : ISabotage
{
    private UnityOptional<PlayerControl> caller;

    public OxygenSabotage(PlayerControl? player = null)
    {
        caller = player == null ? UnityOptional<PlayerControl>.Null() : UnityOptional<PlayerControl>.NonNull(player);
    }

    public SabotageType SabotageType() => Sabotages.SabotageType.Oxygen;

    public bool Fix(PlayerControl? fixer = null)
    {
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(LotusActionType.SabotageFixed, ref handle, this, fixer == null ? PlayerControl.LocalPlayer : fixer);
        if (handle.IsCanceled) return false;

        if (!ShipStatus.Instance.TryGetSystem(SabotageType().ToSystemType(), out ISystemType? systemInstance)) return false;
        LifeSuppSystemType? oxygen = systemInstance!.TryCast<LifeSuppSystemType>();
        if (oxygen == null)
        {
            VentLogger.Warn($"Error Fixing Oxygen Sabotage. Invalid System Cast from {SabotageType()}.");
            return false;
        }

        oxygen.Countdown = 10000f;
        SabotagePatch.CurrentSabotage = null;
        Hooks.SabotageHooks.SabotageFixedHook.Propagate(new SabotageFixHookEvent(fixer, this));
        return true;
    }

    public Optional<PlayerControl> Caller() => caller;

    public void CallSabotage(PlayerControl sabotageCaller)
    {
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(LotusActionType.SabotageStarted, ref handle, this, sabotageCaller);
        if (handle.IsCanceled) return;

        ShipStatus.Instance.RepairSystem(SabotageType().ToSystemType(), sabotageCaller, 128);
        caller.OrElseSet(() => sabotageCaller);
        SabotagePatch.CurrentSabotage = this;
    }

    public override string ToString() => $"OxygenSabotage(Caller={caller})";
}