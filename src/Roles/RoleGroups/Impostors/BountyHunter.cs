using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Impostors;

// TODO: Make into morphling
public class BountyHunter: Impostor
{
    private PlayerControl? bhTarget;
    private Cooldown acquireNewTarget;

    private float bountyKillCoolDown;
    private float punishKillCoolDown;

    [UIComponent(UI.Text)]
    private string ShowTarget() => Color.red.Colorize("Target: ") + Color.white.Colorize(bhTarget == null ? "None" : bhTarget.name);

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        SendKillCooldown(bhTarget?.PlayerId == target.PlayerId);
        bool success = base.TryKill(target);
        if (success)
            BountyHunterAcquireTarget();
        return success;
    }

    [RoleAction(RoleActionType.FixedUpdate)]
    private void BountyHunterTargetUpdate()
    {
        if (acquireNewTarget.NotReady()) return;
        BountyHunterAcquireTarget();
    }

    [RoleAction(RoleActionType.RoundStart)]
    private void BountyHunterTargetOnRoundStart() => BountyHunterAcquireTarget();

    private void BountyHunterAcquireTarget()
    {
        List<PlayerControl> eligiblePlayers = Game.GetAlivePlayers()
            .Where(p => p.Relationship(MyPlayer) is not Relation.FullAllies)
            .ToList();
        if (eligiblePlayers.Count == 0)
        {
            bhTarget = null;
            return;
        }

        // Small function to assign a NEW random target unless there's only one eligible target alive
        PlayerControl newTarget = eligiblePlayers.PopRandom();
        while (eligiblePlayers.Count > 1 && bhTarget?.PlayerId == newTarget.PlayerId)
            newTarget = eligiblePlayers.PopRandom();

        bhTarget = newTarget;
        acquireNewTarget.Start();
    }

    private void SendKillCooldown(bool decreased)
    {
        float cooldown = decreased ? bountyKillCoolDown : punishKillCoolDown;
        GameOptionOverride[] modifiedCooldown = { new(Override.KillCooldown, cooldown) };
        DesyncOptions.SendModifiedOptions(modifiedCooldown, MyPlayer);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Color(RoleColor)
            .SubOption(sub => sub
                .Name("Time Until New Target")
                .Bind(v => acquireNewTarget.Duration = (float)v)
                .AddFloatRange(5f, 120, 5, 11)
                .IOSettings(settings => settings.UnknownValueAction = ADEAnswer.Allow)
                .Build())
            .SubOption(sub => sub
                .Name("Kill Cooldown After Killing Target")
                .Bind(v => bountyKillCoolDown = (float)v)
                .AddFloatRange(0, 180, 2.5f, 6)
                .IOSettings(settings => settings.UnknownValueAction = ADEAnswer.Allow)
                .Build())
            .SubOption(sub => sub
                .Name("Kill Cooldown After Killing Other")
                .Bind(v => punishKillCoolDown = (float)v)
                .AddFloatRange(0, 180, 2.5f, 15)
                .IOSettings(settings => settings.UnknownValueAction = ADEAnswer.Allow)
                .Build());
}