using AmongUs.GameOptions;
using Lotus.Factions;
using Lotus.Factions.Impostors;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Egoist: Shapeshifter
{
    private bool egoistIsShapeshifter;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    public override Relation Relationship(CustomRole role)
    {
        return role.Faction is ImpostorFaction ? Relation.FullAllies : base.Relationship(role);
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
            .Faction(FactionInstances.Solo)
            .RoleColor(new Color(0.34f, 0f, 1f));
}