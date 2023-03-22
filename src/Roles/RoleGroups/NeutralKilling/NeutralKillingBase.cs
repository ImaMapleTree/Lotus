using TOHTOR.Factions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Interfaces;
using UnityEngine;

namespace TOHTOR.Roles.RoleGroups.NeutralKilling;

public partial class NeutralKillingBase: Vanilla.Impostor, IModdable
{
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).SpecialType(SpecialType.NeutralKilling).Faction(FactionInstances.Solo).RoleColor(Color.gray);
}