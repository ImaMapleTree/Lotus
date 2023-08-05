using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.API.Vanilla.Sabotages;

public class DoorSabotage : ISabotage
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(DoorSabotage));

    public SystemTypes Room { get; }
    private Optional<int> doorIndex;
    private UnityOptional<PlayerControl> caller;

    public DoorSabotage(SystemTypes? room, int doorIndex = -1, PlayerControl? caller = null)
    {
        Room = room ?? ShipStatus.Instance.AllDoors[doorIndex != -1 ? doorIndex : 0].Room;
        this.doorIndex = doorIndex != -1 ? Optional<int>.NonNull(doorIndex) : Optional<int>.Null();
        this.caller = caller == null ? UnityOptional<PlayerControl>.Null() : UnityOptional<PlayerControl>.NonNull(caller);
    }

    public SabotageType SabotageType() => Sabotages.SabotageType.Door;

    public bool Fix(PlayerControl? fixer = null)
    {
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(LotusActionType.SabotageFixed, ref handle, this, fixer == null ? PlayerControl.LocalPlayer : fixer);
        if (handle.IsCanceled) return false;

        Hooks.SabotageHooks.SabotageFixedHook.Propagate(new SabotageFixHookEvent(fixer, this));

        return doorIndex.Transform(index => {
            if (index >= ShipStatus.Instance.AllDoors.Length) {
                log.Warn($"Targeted door was out of range ({index})", "FixDoor");
                return false;
            }
            ShipStatus.Instance.AllDoors[index].SetDoorway(true);
            return true;
        }, () => FixRoom(fixer, false));
    }

    public bool FixRoom(PlayerControl? fixer = null, bool sendAction = true)
    {
        if (sendAction)
        {
            ActionHandle handle = ActionHandle.NoInit();
            Game.TriggerForAll(LotusActionType.SabotageFixed, ref handle, this, fixer == null ? PlayerControl.LocalPlayer : fixer);
            Hooks.SabotageHooks.SabotageFixedHook.Propagate(new SabotageFixHookEvent(fixer, this));
            if (handle.IsCanceled) return false;
        }

        ShipStatus.Instance.AllDoors.Where(d => d.Room == Room).ForEach(door => door.SetDoorway(true));
        return true;
    }

    public Optional<PlayerControl> Caller() => caller;

    public void CallSabotage(PlayerControl sabotageCaller)
    {
        ShipStatus.Instance.AllDoors.Where(d => d.Room == Room).ForEach(door => door.SetDoorway(false));
    }
}