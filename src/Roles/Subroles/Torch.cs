using System.Collections.Generic;
using Lotus.API;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace Lotus.Roles.Subroles;

public class Torch: Subrole
{
    private static readonly HashSet<IFaction> ImpostorFaction = new() { FactionInstances.Impostors };

    public override string Identifier() => "☀";

    [RoleAction(RoleActionType.SabotageStarted)]
    [RoleAction(RoleActionType.SabotageFixed)]
    private void AdjustSabotageVision(ActionHandle handle)
    {
        VentLogger.Trace("Fixing Player Vision", "Torch");
        Async.Schedule(SyncOptions, handle.ActionType is RoleActionType.SabotageStarted ? 1f : 0.2f);
    }

    public override HashSet<IFaction> RegulatedFactions() => ImpostorFaction;

    public override CompatabilityMode FactionCompatabilityMode => CompatabilityMode.Blacklisted;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => AddRestrictToCrew(base.RegisterOptions(optionStream));

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(1f, 0.6f, 0.08f))
            .OptionOverride(Override.CrewLightMod, () => AUSettings.CrewLightMod() * 5,
                () => SabotagePatch.CurrentSabotage != null && SabotagePatch.CurrentSabotage.SabotageType() is SabotageType.Lights);
}