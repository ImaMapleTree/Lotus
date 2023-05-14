using System.Collections.Generic;
using Lotus.API;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using UnityEngine;

namespace Lotus.Roles.Subroles;

public class Torch: Subrole
{
    private static readonly HashSet<IFaction> ImpostorFaction = new() { FactionInstances.Impostors };
    
    public override string Identifier() => "☀";
    
    [RoleAction(RoleActionType.SabotageStarted)]
    [RoleAction(RoleActionType.SabotageFixed)]
    private void AdjustSabotageVision() => SyncOptions();

    public override HashSet<IFaction> RegulatedFactions() => ImpostorFaction;

    public override CompatabilityMode FactionCompatabilityMode => CompatabilityMode.Blacklisted;

    protected override RoleModifier Modify(RoleModifier roleModifier) => 
        base.Modify(roleModifier)
            .RoleColor(new Color(1f, 0.71f, 0.56f))
            .OptionOverride(Override.CrewLightMod, () => AUSettings.ImpostorLightMod(), 
                () => SabotagePatch.CurrentSabotage != null && SabotagePatch.CurrentSabotage.SabotageType() is SabotageType.Lights);
}