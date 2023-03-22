using System;
using System.Linq;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.API.Vanilla.Sabotages;
using TOHTOR.Extensions;
using TOHTOR.Gamemodes;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Logging;
using Impostor = TOHTOR.Roles.RoleGroups.Vanilla.Impostor;

namespace TOHTOR.Patches.Systems;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RepairSystem))]
public static class SabotagePatch
{
    public static float SabotageCountdown = -1;
    public static ISabotage? CurrentSabotage;

    internal static bool Prefix(ShipStatus __instance,
        [HarmonyArgument(0)] SystemTypes systemType,
        [HarmonyArgument(1)] PlayerControl player,
        [HarmonyArgument(2)] byte amount)
    {
        ActionHandle handle = ActionHandle.NoInit();
        ISystemType systemInstance;
        VentLogger.Trace($"Repair System: {systemType} | Player: {player.UnalteredName()} | Amount: {amount}");
        switch (systemType)
        {
            case SystemTypes.Sabotage:
                if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.CallSabotage)) return false;
                if (player.GetCustomRole() is Impostor impostor && !impostor.CanSabotage()) return false;
                SabotageCountdown = -1;
                SabotageType sabotage = (SystemTypes)amount switch
                {
                    SystemTypes.Electrical => SabotageType.Lights,
                    SystemTypes.Comms => SabotageType.Communications,
                    SystemTypes.LifeSupp => SabotageType.Oxygen,
                    SystemTypes.Reactor => SabotageType.Reactor,
                    SystemTypes.Laboratory => SabotageType.Reactor,
                    _ => throw new Exception("Invalid Sabotage Type")
                };
                ISabotage sabo = ISabotage.From(sabotage, player);
                Game.TriggerForAll(RoleActionType.SabotageStarted, ref handle, sabo, player);
                if (!handle.IsCanceled) CurrentSabotage = sabo;
                break;
            case SystemTypes.Electrical:
                if (amount > 64) return true;
                if (CurrentSabotage?.SabotageType() != SabotageType.Lights) break;
                if (!__instance.TryGetSystem(systemType, out systemInstance)) break;
                SwitchSystem electrical = systemInstance!.Cast<SwitchSystem>();
                byte currentSwitches = electrical.ActualSwitches;
                if (amount.HasBit(128))
                    currentSwitches ^= (byte) (amount & 31U);
                else
                    currentSwitches ^= (byte) (1U << amount);
                if (currentSwitches != electrical.ExpectedSwitches)
                {
                    Game.TriggerForAll(RoleActionType.SabotagePartialFix, ref handle, CurrentSabotage, player);
                    break;
                }
                VentLogger.Info($"Electrical Sabotage Fixed by {player.UnalteredName()}", "SabotageFix");
                Game.TriggerForAll(RoleActionType.SabotageFixed, ref handle, CurrentSabotage, player);
                CurrentSabotage = null;
                break;
            case SystemTypes.Comms:
                if (CurrentSabotage?.SabotageType() != SabotageType.Communications) break;
                if (!__instance.TryGetSystem(systemType, out systemInstance)) break;
                if (systemInstance.TryCast<HudOverrideSystemType>() != null && amount == 0)
                {
                    Game.TriggerForAll(RoleActionType.SabotageFixed, ref handle, CurrentSabotage, player);
                    CurrentSabotage = null;
                } else if (systemInstance.TryCast<HqHudSystemType>() != null) // Mira has a special communications which requires two people
                {
                    HqHudSystemType miraComms = systemInstance.Cast<HqHudSystemType>(); // Get mira comm instance
                    byte commsNum = (byte) (amount & 15U); // Convert to 0 or 1 for respective console
                    if (miraComms.CompletedConsoles.Contains(commsNum)) break; // Negative check if console has already been fixed (refreshes periodically)

                    // Send partial fix action
                    Game.TriggerForAll(RoleActionType.SabotagePartialFix, ref handle, CurrentSabotage, player);
                    // If there's more than 1 already fixed then comms is fixed totally
                    if (miraComms.NumComplete == 0) break;
                    Game.TriggerForAll(RoleActionType.SabotageFixed, ref handle, CurrentSabotage, player);
                    CurrentSabotage = null;
                }
                if (CurrentSabotage == null)
                    VentLogger.Info($"Communications Sabotage Fixed by {player.UnalteredName()}", "SabotageFix");
                break;
            case SystemTypes.LifeSupp:
                if (CurrentSabotage?.SabotageType() != SabotageType.Oxygen) break;
                if (!__instance.TryGetSystem(systemType, out systemInstance)) break;
                LifeSuppSystemType oxygen = systemInstance!.Cast<LifeSuppSystemType>();
                int o2Num = amount & 3;
                if (oxygen.CompletedConsoles.Contains(o2Num)) break;
                Game.TriggerForAll(RoleActionType.SabotagePartialFix, ref handle, CurrentSabotage, player);
                if (oxygen.UserCount == 0) break;
                Game.TriggerForAll(RoleActionType.SabotageFixed, ref handle, CurrentSabotage, player);
                CurrentSabotage = null;
                VentLogger.Info($"Oxygen Sabotage Fixed by {player.UnalteredName()}", "SabotageFix");
                break;
            case SystemTypes.Laboratory:
            case SystemTypes.Reactor:
                if (CurrentSabotage?.SabotageType() != SabotageType.Reactor) break;
                if (!__instance.TryGetSystem(systemType, out systemInstance)) break;
                ReactorSystemType reactor = systemInstance!.Cast<ReactorSystemType>();
                int reactNum = amount & 3;
                if (reactor.UserConsolePairs.ToList().Any(p => p.Item2 == reactNum)) break;
                Game.TriggerForAll(RoleActionType.SabotagePartialFix, ref handle, CurrentSabotage, player);
                if (reactor.UserCount == 0) break;
                Game.TriggerForAll(RoleActionType.SabotageFixed, ref handle, CurrentSabotage, player);
                CurrentSabotage = null;
                VentLogger.Info($"Reactor Sabotage Fixed by {player.UnalteredName()}", "SabotageFix");
                break;
            default:
                return true;
        }

        return !handle.IsCanceled;
    }
}