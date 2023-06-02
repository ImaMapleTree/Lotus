using Lotus.Factions;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using UnityEngine;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public partial class NeutralKillingBase: Vanilla.Impostor, IModdable
{
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .SpecialType(SpecialType.NeutralKilling)
            .Faction(FactionInstances.Solo)
            .RoleColor(Color.gray);
}