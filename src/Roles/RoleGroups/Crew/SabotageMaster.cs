using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.API.Vanilla.Sabotages;
using TOHTOR.Extensions;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Crew;

public class SabotageMaster: Crewmate
{
    private List<SabotageType> sabotages = new();



    [RoleAction(RoleActionType.SabotagePartialFix)]
    private void SaboMasterFixes(ISabotage sabotage, PlayerControl fixer)
    {
        if (fixer.PlayerId != MyPlayer.PlayerId || !sabotages.Contains(sabotage.SabotageType())) return;
        sabotage.Fix(MyPlayer);
        Game.GameHistory.AddEvent(new GenericAbilityEvent(MyPlayer, $"{ModConstants.HColor1.Colorize(MyPlayer.UnalteredName())} fixed {sabotage.SabotageType()}."));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Fast Fixes Lights")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Lights))
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Reactor")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Lights))
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Oxygen")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Oxygen))
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Comms")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Communications))
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Doors")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Door))
                .Build())
            .SubOption(sub => sub.Name("Fast Fixes Helicopter")
                .AddOnOffValues()
                .BindBool(RoleUtils.BindOnOffListSetting(sabotages, SabotageType.Helicopter))
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(Color.blue);
}