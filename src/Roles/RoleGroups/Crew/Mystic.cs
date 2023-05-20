using Lotus.API.Vanilla.Sabotages;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Roles.Subroles;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Crew;

public class Mystic : Crewmate
{
    private float flashDuration;
    private bool sendAudioAlert;

    [RoleAction(RoleActionType.AnyDeath)]
    private void MysticAnyDeath(PlayerControl deadPlayer)
    {
        if (MyPlayer.Data.IsDead) return;
        if (deadPlayer.GetSubrole<Bait>() != null) return;
        
        
        GameOptionOverride[] overrides = { new(Override.CrewLightMod, 0f) };
        SyncOptions(overrides);

        bool didReactorAlert = false;
        if (sendAudioAlert && SabotagePatch.CurrentSabotage?.SabotageType() is not SabotageType.Reactor)
        {
            RoleUtils.PlayReactorsForPlayer(MyPlayer);
            didReactorAlert = true;
        }

        Async.Schedule(() => MysticRevertAlert(didReactorAlert), NetUtils.DeriveDelay(flashDuration));

    }

    private void MysticRevertAlert(bool didReactorAlert)
    {
        SyncOptions();
        if (!didReactorAlert) return;
        RoleUtils.EndReactorsForPlayer(MyPlayer);
    }


    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Flash Duration")
                .Bind(v => flashDuration = (float)v)
                .AddFloatRange(0.2f, 1.5f, 0.1f, 4, "s")
                .Build())
            .SubOption(sub => sub
                .Name("Send Audio Alert")
                .BindBool(v => sendAudioAlert = v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.3f, 0.6f, 0.9f));
}