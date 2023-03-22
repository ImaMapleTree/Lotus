using Il2CppSystem;
using TOHTOR.Extensions;
using TOHTOR.Patches.Systems;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Utilities.Optionals;

namespace TOHTOR.API.Vanilla.Sabotages;

public class CommsSabotage : ISabotage
{
    private UnityOptional<PlayerControl> caller;

    public CommsSabotage(PlayerControl? player = null)
    {
        caller = player == null ? UnityOptional<PlayerControl>.Null() : UnityOptional<PlayerControl>.NonNull(player);
    }

    public SabotageType SabotageType() => Sabotages.SabotageType.Communications;

    public bool Fix(PlayerControl? fixer = null)
    {

        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.SabotageFixed, ref handle, this, fixer == null ? Optional<PlayerControl>.Null() : Optional<PlayerControl>.Of(fixer));
        if (handle.IsCanceled) return false;
        fixer = fixer == null ? PlayerControl.LocalPlayer : fixer;


        if (!ShipStatus.Instance.TryGetSystem(SabotageType().ToSystemType(), out ISystemType? systemInstance)) return false;
        if (systemInstance!.TryCast<HudOverrideSystemType>() != null)
        {
            HudOverrideSystemType hudOverrideSystemType = systemInstance.Cast<HudOverrideSystemType>();
            hudOverrideSystemType.RepairDamage(fixer, 0);
        } else if (systemInstance.TryCast<HqHudSystemType>() != null) // Mira has a special communications which requires two people
        {
            HqHudSystemType miraComms = systemInstance.Cast<HqHudSystemType>(); // Get mira comm instance
            miraComms.CompletedConsoles.Add(0);
            miraComms.CompletedConsoles.Add(1);
            miraComms.ActiveConsoles.Add(new Tuple<byte, byte>(fixer.PlayerId, 0));
            miraComms.ActiveConsoles.Add(new Tuple<byte, byte>(fixer.PlayerId, 1));
            miraComms.IsDirty = true;
        }

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
}