using TOHTOR.Extensions;
using TOHTOR.Patches.Systems;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Logging;
using VentLib.Utilities.Optionals;

namespace TOHTOR.API.Vanilla.Sabotages;

public class ElectricSabotage : ISabotage
{
    private UnityOptional<PlayerControl> caller;

    public ElectricSabotage(PlayerControl? player = null)
    {
        caller = player == null ? UnityOptional<PlayerControl>.Null() : UnityOptional<PlayerControl>.NonNull(player);
    }

    public SabotageType SabotageType() => Sabotages.SabotageType.Lights;

    public bool Fix(PlayerControl? fixer = null)
    {
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.SabotageFixed, ref handle, this, fixer == null ? Optional<PlayerControl>.Null() : Optional<PlayerControl>.Of(fixer));
        if (handle.IsCanceled) return false;

        if (!ShipStatus.Instance.TryGetSystem(SabotageType().ToSystemType(), out ISystemType? systemInstance)) return false;
        SwitchSystem? electrical = systemInstance!.TryCast<SwitchSystem>();
        if (electrical == null)
        {
            VentLogger.Warn($"Error Fixing Lights Sabotage. Invalid System Cast from {SabotageType()}.");
            return false;
        }
        electrical.ActualSwitches = electrical.ExpectedSwitches;
        SabotagePatch.CurrentSabotage = null;
        return true;
    }

    public Optional<PlayerControl> Caller() => caller;

    public void Sabotage(PlayerControl sabotageCaller)
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