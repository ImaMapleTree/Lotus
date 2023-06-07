using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.NeutralKilling;
using Lotus.Victory;
using Lotus.Victory.Conditions;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Factions.Neutrals;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Hitman: NeutralKillingBase
{
    private static HitmanFaction _hitmanFaction = new HitmanFaction();
    public List<string> AdditionalWinRoles = new();

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        Game.GetWinDelegate().AddSubscriber(GameEnd);
    }

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    private void GameEnd(WinDelegate winDelegate)
    {
        if (!MyPlayer.IsAlive()) return;
        if (winDelegate.GetWinReason() is WinReason.SoloWinner && !AdditionalWinRoles.Contains(winDelegate.GetWinners()[0].GetCustomRole().EnglishRoleName)) return; 
        winDelegate.GetWinners().Add(MyPlayer);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Wins with Absolute Winners")
                .Value(v => v.Text("None").Color(Color.red).Value(0).Build())
                .Value(v => v.Text("All").Color(Color.cyan).Value(1).Build())
                .Value(v => v.Text("Individual").Color(new Color(0.45f, 0.31f, 0.72f)).Value(2).Build())
                .ShowSubOptionPredicate(o => (int)o == 2)
                .SubOption(sub2 => sub2
                    .Name("Executioner")
                    .Color(new Color(0.55f, 0.17f, 0.33f))
                    .AddOnOffValues()
                    .BindBool(RoleUtils.BindOnOffListSetting(AdditionalWinRoles, "Executioner"))
                    .Build())
                .SubOption(sub2 => sub2
                    .Name("Jester")
                    .Color(new Color(0.93f, 0.38f, 0.65f))
                    .AddOnOffValues()
                    .BindBool(RoleUtils.BindOnOffListSetting(AdditionalWinRoles, "Jester"))
                    .Build())
                .SubOption(sub2 => sub2
                    .Name("Lovers")
                    .Color(new Color(1f, 0.4f, 0.8f))
                    .AddOnOffValues()
                    .BindBool(RoleUtils.BindOnOffListSetting(AdditionalWinRoles, "Lovers"))
                    .Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).Faction(_hitmanFaction);


    private class HitmanFaction : Factions.Neutrals.Neutral
    {
        public override Relation Relationship(Factions.Neutrals.Neutral sameFaction)
        {
            return Relation.SharedWinners;
        }

        public override Relation RelationshipOther(IFaction other)
        {
            return Relation.SharedWinners;
        }
    }
}