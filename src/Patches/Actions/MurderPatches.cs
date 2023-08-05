using System;
using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Managers.History.Events;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Internals.Enums;
using Lotus.Server;
using Lotus.Utilities;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;
using VentLib.Utilities.Optionals;

namespace Lotus.Patches.Actions;


public static class MurderPatches
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(MurderPatches));

    public static PlayerControl LastAttacker = null!;
    private static readonly Dictionary<byte, FixedUpdateLock> MurderLocks = new();
    private static readonly Func<FixedUpdateLock> TimeoutSupplier = () => new FixedUpdateLock(0.25f);

    public static bool Lock(byte player) => MurderLocks.GetOrCompute(player, TimeoutSupplier).AcquireLock(NetUtils.DeriveDelay(0.25f));

    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    public static bool Prefix(PlayerControl __instance, PlayerControl target)
    {
        if (ClientOptions.AdvancedOptions.PublicCompatability) return true;
        if (!AmongUsClient.Instance.AmHost) return false;
        if (__instance == null || target == null) return false;

        log.Debug($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}", "CheckMurder");


        if (target.Data == null || target.inVent || target.inMovingPlat)
        {
            log.Trace($"Unable to kill {target.name}. Invalid Status", "CheckMurder");
            return false;
        }
        if (!target.IsAlive())
        {
            log.Trace($"Unable to kill {target.name}. Player is already dead.", "CheckMurder");
            return false;
        }
        if (MeetingHud.Instance != null)
        {
            log.Trace($"Unable to kill {target.name}. There is currently a meeting.", "CheckMurder");
            return false;
        }

        if (__instance.PlayerId == target.PlayerId) return false;

        if (!MurderLocks.GetOrCompute(__instance.PlayerId, TimeoutSupplier).IsUnlocked()) return false;

        ActionHandle handle = ActionHandle.NoInit();
        __instance.Trigger(LotusActionType.Attack, ref handle, target);
        return false;
    }

    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static void SaveAttacker(PlayerControl __instance)
    {
        LastAttacker = __instance;
    }

    [QuickPostfix(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static void MurderPlayer(PlayerControl __instance, PlayerControl target) => ServerPatchManager.Patch.Execute(PatchedCode.MurderPlayer, __instance, target);
}