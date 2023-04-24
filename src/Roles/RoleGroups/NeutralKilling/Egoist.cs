using AmongUs.GameOptions;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Impostors;
using TOHTOR.Options;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;

namespace TOHTOR.Roles.RoleGroups.NeutralKilling;

public class Egoist: Shapeshifter
{
    private bool egoistIsShapeshifter;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    public override Relation Relationship(PlayerControl player)
    {
        return player.GetCustomRole().Faction is ImpostorFaction ? Relation.FullAllies : base.Relationship(player);
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