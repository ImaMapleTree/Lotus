using System.Linq;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Factions.Impostors;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Factions.Interfaces;
using Lotus.Options.Roles;
using Lotus.Roles.Internals;
using Lotus.Victory;
using Lotus.Victory.Conditions;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Egoist: Shapeshifter
{
    private static EgoistFaction _egoistFaction = new();
    private bool egoistIsShapeshifter;

    protected override void PostSetup() => Game.GetWinDelegate().AddSubscriber(PreventImpostorWinCondition);

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    public override Relation Relationship(CustomRole role)
    {
        return role.Faction is ImpostorFaction ? Relation.None : base.Relationship(role);
    }

    private void PreventImpostorWinCondition(WinDelegate winDelegate)
    {
        if (MyPlayer == null) return;
        if (winDelegate.WinCondition() is not IFactionWinCondition factionWin) return;
        if (factionWin.Factions().All(f => f is not ImpostorFaction)) return;

        if (!MyPlayer.IsAlive()) winDelegate.GetWinners().RemoveAll(w => w.PlayerId == MyPlayer.PlayerId);
        else winDelegate.CancelGameWin();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub.Name("Egoist is Shapeshifter")
                .BindBool(b => egoistIsShapeshifter = b)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(egoistIsShapeshifter ? RoleTypes.Shapeshifter : RoleTypes.Impostor)
            .Faction(_egoistFaction)
            .SpecialType(SpecialType.NeutralKilling)
            .RoleColor(new Color(0.34f, 0f, 1f));

    private class EgoistFaction : Faction<EgoistFaction>
    {
        public override Relation Relationship(EgoistFaction sameFaction)
        {
            return Options.RoleOptions.NeutralOptions.NeutralTeamingMode is not NeutralTeaming.Disabled
                ? Relation.FullAllies
                : Relation.None;
        }

        public override bool CanSeeRole(PlayerControl player)
        {
            return false;
        }

        public override Color FactionColor() => new(0.34f, 0f, 1f);

        public override Relation RelationshipOther(IFaction other)
        {
            return other is ImpostorFaction ? Relation.FullAllies : Relation.None;
        }
    }
}